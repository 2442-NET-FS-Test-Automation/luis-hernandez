namespace Library.Data.Entities;

public enum Priority
{
    //By default enums are backed by ordinals (0,1,2,etc)
    //We can give the values explicitly if we're going to do math or sort based on the enums
    Normal = 0,
    Expedited = 1
}