using Microsoft.EntityFrameworkCore;
using Pharmacy.Data;
using Pharmacy.Data.Configurations;
using Serilog;

namespace Pharmacy.Api.Services;

/// <summary>
/// Operaciones de asignación de Dispatcher reutilizables entre
/// DispatcherWorkerService (un loop por Dispatcher conocido) y
/// FulfillmentService.FulfillBurstAsync (necesita "cualquier" Dispatcher libre).
/// No se registra en DI: son funciones puras sobre un PharmacyDbContext que
/// ya te pasan, para no forzar un ciclo de vida propio.
/// </summary>
public static class DispatcherAllocation
{
    /// <summary>Marca un Dispatcher específico como Busy, si estaba Free.</summary>
    public static async Task<bool> TryMarkBusyAsync(PharmacyDbContext db, int dispatcherId, CancellationToken ct)
    {
        var dispatcher = await db.Dispatchers.FindAsync(new object?[] { dispatcherId }, ct);
        if (dispatcher is null || dispatcher.Status == DispatcherStatus.Busy)
        {

            return false;
        }

        dispatcher.Status = DispatcherStatus.Busy;

        try
        {
            await db.SaveChangesAsync(ct);
            Log.Information("Dispatcher {dispatcherId} is marked as busy", dispatcherId);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Otro proceso lo tomó primero entre el FindAsync y el SaveChanges.
            return false;
        }
    }

    /// <summary>Libera un Dispatcher específico, marcándolo Free.</summary>
    public static async Task MarkFreeAsync(PharmacyDbContext db, int dispatcherId, CancellationToken ct)
    {
        var dispatcher = await db.Dispatchers.FindAsync(new object?[] { dispatcherId }, ct);
        if (dispatcher is null) return;

        dispatcher.Status = DispatcherStatus.Free;

        try
        {
            await db.SaveChangesAsync(ct);
            Log.Information("Dispatcher {dispatcherId} is marked as free", dispatcherId);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Best-effort: si hay conflicto, el Dispatcher queda en el estado
            // que tenga en BD; no es crítico bloquear el flujo por esto.
        }
    }

    /// <summary>Busca cualquier Dispatcher Free y lo marca Busy atómicamente (con reintento simple).</summary>
    public static async Task<int?> TryAcquireAnyFreeAsync(PharmacyDbContext db, CancellationToken ct)
    {
        var candidate = await db.Dispatchers
            .Where(d => d.Status == DispatcherStatus.Free)
            .Select(d => d.Id)
            .FirstOrDefaultAsync(ct);

        if (candidate == 0) return null;

        return await TryMarkBusyAsync(db, candidate, ct) ? candidate : null;
    }
}