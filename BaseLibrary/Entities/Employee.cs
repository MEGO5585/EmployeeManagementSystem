using System;

namespace BaseLibrary.Entities;

public class Employee
{
    public int Id{ get; set; }
    public string? Name { get; set; }
    public string? CivilId{ get; set; }
    public string? FileNumber{ get; set; }
    public string? FullName{ get; set; }
    public string? JobName{ get; set; }
    public string? Adress{ get; set; }
    public string? TelephoneNumber{ get; set; }
    public string? Photo{ get; set; }
    public string? Other { get; set; }
    
    // Relationships Many to One
    public GeneralDepartment? generalDepartment { get; set; }
    public int generalDepartmentId { get; set; }
    public Department? department { get; set; }
    public int departmentId { get; set; }
    public Bransh? bransh { get; set; }
    public int branshId { get; set; }
    public Town? town { get; set; }
    public int townId{ get; set; }

}
