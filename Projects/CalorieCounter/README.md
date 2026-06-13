# CalorieCounter

A personal nutrition tracking application built with C# and .NET.

The purpose of this project is to help users monitor their daily calorie and macronutrient intake while serving as a practical platform for learning software engineering concepts, object-oriented programming, and modern .NET development practices.

---

## Project Goals

The application allows users to:

- Track daily calorie consumption.
- Track macronutrient intake:
  - Protein
  - Carbohydrates
  - Fat
- Record meals consumed throughout the day.
- Create reusable recipes composed of multiple foods.
- Maintain historical nutrition records over time.

---

## Core Features

### Food Management

Manage nutritional information for individual food items.

Each food stores:

- Name
- Reference serving size
- Calories
- Protein
- Carbohydrates
- Fat

Supported operations:

- Create
- Read
- Update
- Delete

---

### Recipe Management

Create recipes composed of one or more foods.

Each recipe contains:

- A collection of foods
- Quantities for each food
- Calculated nutritional values

Supported operations:

- Create
- Read
- Update
- Delete

---

### Meal Tracking

Track foods and recipes consumed during a meal.

Examples:

- Breakfast
- Lunch
- Dinner
- Snack

The nutritional values of a meal are calculated from all foods and recipes it contains.

Supported operations:

- Create
- Read
- Update
- Delete

---

### Daily Nutrition Tracking

Track nutritional intake for a specific day.

The system calculates:

- Total calories
- Total protein
- Total carbohydrates
- Total fat

Supported operations:

- Create
- Read
- Update
- Delete

---

## Nutritional Calculation Rules

### Food Consumption

Nutritional values are calculated proportionally based on a food's reference serving size.

Example:

| Property | Value |
|----------|--------|
| Reference Serving | 100 g |
| Calories | 250 kcal |
| Consumed Amount | 50 g |

Result:

| Property | Value |
|----------|--------|
| Calories Consumed | 125 kcal |

The same proportional calculation applies to protein, carbohydrates, and fat.

---

### Recipe Consumption

A recipe's nutritional values are calculated from the combined nutritional values of all included foods.

When only a portion of a recipe is consumed, nutritional values are calculated proportionally according to the consumed amount relative to the recipe's total quantity.

---

## Domain Model

The current domain revolves around the following core entities:

```text
Food
 └─ Nutritional Information

Recipe
 └─ Contains Foods

Meal
 ├─ Contains Foods
 └─ Contains Recipes

DailyRecord
 └─ Contains Meals
```

---

## Learning Objectives

This repository is also used as a hands-on learning project for:

- C#
- .NET CLI
- Object-Oriented Programming (OOP)
- SOLID Principles
- Design Patterns
- Collections and Generics
- Exception Handling
- Unit Testing
- Async/Await
- HTTP APIs
- Data Structures and Algorithms

Progress is tracked in:

```text
LEARNING_CHECKLIST.md
```

---

## Project Structure

```text
CalorieCounter/
│
├── README.md
├── LEARNING_CHECKLIST.md
│
├── CalorieCounter.Console/
└── CalorieCounter.Domain/
```

---

## Running the Application

Build the solution:

```bash
dotnet build
```

Run the console application:

```bash
dotnet run --project CalorieCounter.Console
```

---

## Future Enhancements

Planned features include:

- User profiles
- Calorie targets
- Macronutrient goals
- Weight tracking
- Progress reports
- Persistence layer
- Integration with external nutrition databases

---

## License

This project is intended for educational purposes and personal learning.