using PatientWorklist.API.Data;
using PatientWorklist.API.Entities;

namespace PatientWorklist.API.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Doctors.Any())
        {
            return;
        }

        var doctors = new List<Doctor>
        {
            new Doctor
            {
                Specialty = "Radiology",
                Person = new Person
                {
                    FirstName = "Sarah",
                    LastName = "Mitchell",
                    DateOfBirth = new DateTime(1978, 3, 12),
                    Gender = "Female",
                    Phone = "555-0101",
                    Email = "sarah.mitchell@hospital.com"
                }
            },
            new Doctor
            {
                Specialty = "Cardiology",
                Person = new Person
                {
                    FirstName = "James",
                    LastName = "O'Connor",
                    DateOfBirth = new DateTime(1970, 8, 5),
                    Gender = "Male",
                    Phone = "555-0102",
                    Email = "james.oconnor@hospital.com"
                }
            },
            new Doctor
            {
                Specialty = "Neurology",
                Person = new Person
                {
                    FirstName = "Priya",
                    LastName = "Sharma",
                    DateOfBirth = new DateTime(1984, 11, 23),
                    Gender = "Female",
                    Phone = "555-0103",
                    Email = "priya.sharma@hospital.com"
                }
            }
        };

        var patients = new List<Patient>
        {
            new Patient
            {
                MRN = "MRN-1001",
                Status = "Active",
                Person = new Person
                {
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = new DateTime(1985, 4, 15),
                    Gender = "Male",
                    Phone = "555-0201",
                    Email = "john.doe@example.com"
                }
            },
            new Patient
            {
                MRN = "MRN-1002",
                Status = "Pending",
                Person = new Person
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    DateOfBirth = new DateTime(1990, 9, 30),
                    Gender = "Female",
                    Phone = "555-0202",
                    Email = "jane.smith@example.com"
                }
            },
            new Patient
            {
                MRN = "MRN-1003",
                Status = "Active",
                Person = new Person
                {
                    FirstName = "Ali",
                    LastName = "Khan",
                    DateOfBirth = new DateTime(1976, 1, 8),
                    Gender = "Male",
                    Phone = "555-0203",
                    Email = "ali.khan@example.com"
                }
            },
            new Patient
            {
                MRN = "MRN-1004",
                Status = "Inactive",
                Person = new Person
                {
                    FirstName = "Maria",
                    LastName = "Garcia",
                    DateOfBirth = new DateTime(2001, 12, 22),
                    Gender = "Female",
                    Phone = "555-0204",
                    Email = "maria.garcia@example.com"
                }
            },
            new Patient
            {
                MRN = "MRN-1005",
                Status = "Active",
                Person = new Person
                {
                    FirstName = "Robert",
                    LastName = "Brown",
                    DateOfBirth = new DateTime(1958, 6, 17),
                    Gender = "Male",
                    Phone = "555-0205",
                    Email = "robert.brown@example.com"
                }
            }
        };

        var studies = new List<Study>
        {
            new Study { Patient = patients[0], Doctor = doctors[0], Modality = "CT", StudyDate = new DateTime(2026, 7, 28), Status = "Completed" },
            new Study { Patient = patients[0], Doctor = doctors[1], Modality = "MRI", StudyDate = new DateTime(2026, 8, 2), Status = "Scheduled" },
            new Study { Patient = patients[1], Doctor = doctors[0], Modality = "X-Ray", StudyDate = new DateTime(2026, 8, 5), Status = "In Progress" },
            new Study { Patient = patients[2], Doctor = doctors[2], Modality = "MRI", StudyDate = new DateTime(2026, 8, 10), Status = "Completed" },
            new Study { Patient = patients[3], Doctor = doctors[1], Modality = "Ultrasound", StudyDate = new DateTime(2026, 8, 12), Status = "Scheduled" }
        };

        context.Doctors.AddRange(doctors);
        context.Patients.AddRange(patients);
        context.Studies.AddRange(studies);
        context.SaveChanges();
    }
}