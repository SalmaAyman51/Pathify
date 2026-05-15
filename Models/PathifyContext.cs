using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pathify.Controllers;
using Pathify.Data;

namespace Pathify.Models;

public partial class PathifyContext : IdentityDbContext<ApplicationUser>
{
    public PathifyContext()
    {
    }

    public PathifyContext(DbContextOptions<PathifyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminPhone> AdminPhones { get; set; }

    public virtual DbSet<Adminstration> Adminstrations { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<ExternalProfessor> ExternalProfessors { get; set; }

    public virtual DbSet<ExternalProfessorPhone> ExternalProfessorPhones { get; set; }

    public virtual DbSet<InternalProfessor> InternalProfessors { get; set; }

    public virtual DbSet<InternalProfessorPhone> InternalProfessorPhones { get; set; }

    public virtual DbSet<Level> Levels { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentPhone> StudentPhones { get; set; }

    public virtual DbSet<Supervisor> Supervisors { get; set; }
    public virtual DbSet<TempStudentData> TempStudentData { get; set; }
    public virtual DbSet<SelectedCourse> SelectedCourses { get; set; }
    public virtual DbSet<Team> Teams { get; set; }
    public virtual DbSet<TeamMember> TeamMembers { get; set; }
    public virtual DbSet<TeamLimit> TeamLimits { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=soly;Database=Pathify;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AdminPhone>(entity =>
        {

            entity.HasKey(e => new { e.AdminSsn, e.PhoneNumber }).HasName("PK__Admin_ph__BF0595A377126546");

            entity.ToTable("Admin_phone");

            entity.Property(e => e.AdminSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("AdminSSN");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);

            entity.HasOne(d => d.AdminSsnNavigation).WithMany(p => p.AdminPhones)
                .HasForeignKey(d => d.AdminSsn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Admin_phone_Adminstration");
        });

        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => new { e.TeamId, e.StudentSsn });

            entity.HasOne(e => e.Team)
                .WithMany(t => t.TeamMembers)
                .HasForeignKey(e => e.TeamId);

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentSsn);
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasOne(e => e.Leader)
                .WithMany()
                .HasForeignKey(e => e.LeaderSsn);
        });
        modelBuilder.Entity<Adminstration>(entity =>
        {
            entity.HasKey(e => e.AdminSsn);

            entity.ToTable("Adminstration");

            entity.Property(e => e.AdminSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("AdminSSN");
            entity.Property(e => e.Fname)
                .HasMaxLength(50)
                .HasColumnName("FName");
            entity.Property(e => e.FullName)
                .HasMaxLength(101)
                .HasComputedColumnSql("(([FName]+' ')+[LName])", false);
            entity.Property(e => e.Lname)
                .HasMaxLength(50)
                .HasColumnName("LName");
            entity.Property(e => e.ManagerSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("managerSSN");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.ManagerSsnNavigation).WithMany(p => p.InverseManagerSsnNavigation)
                .HasForeignKey(d => d.ManagerSsn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Adminstration_Adminstration");
        });


        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__37E005DBC529ABC7");

            entity.Property(e => e.CourseId)
                .HasMaxLength(50)
                .HasColumnName("Course_Id");
            entity.Property(e => e.AdminSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("AdminSSN");
            entity.Property(e => e.CourseName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Course_Name");
            entity.Property(e => e.CourseSemester)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Course_semester");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PreReqCourseId)
                .HasMaxLength(50)
                .HasColumnName("PreReqCourseID");

            entity.HasOne(d => d.AdminSsnNavigation).WithMany(p => p.Courses)
                .HasForeignKey(d => d.AdminSsn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Courses_Adminstration");

            entity.HasOne(d => d.CourseLevelNavigation).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CourseLevel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Courses_Levels");

            entity.HasOne(d => d.PreReqCourse).WithMany(p => p.InversePreReqCourse)
                .HasForeignKey(d => d.PreReqCourseId)
                .HasConstraintName("FK_Courses_Courses");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => new { e.CourseId, e.StudentSsn }).HasName("PK__Enrollme__BDD4E4F595056AF2");

            entity.ToTable("Enrollment");

            entity.Property(e => e.CourseId)
                .HasMaxLength(50)
                .HasColumnName("Course_Id");
            entity.Property(e => e.StudentSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("StudentSSN");
            entity.Property(e => e.AdminSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("AdminSSN");
            entity.Property(e => e.EnrollmentDate).HasColumnName("Enrollment_Date");
            entity.Property(e => e.Passed).HasColumnName("passed");

            entity.HasOne(d => d.AdminSsnNavigation).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.AdminSsn)
                .HasConstraintName("FK_Enrollment_Adminstration");

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrollment_Courses");

            entity.HasOne(d => d.StudentSsnNavigation).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentSsn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrollment_Students");
        });

        modelBuilder.Entity<ExternalProfessor>(entity =>
        {
            entity.HasKey(e => e.ExternalProfessorSsn).HasName("PK__External__2CA6D419ACC08BAD");

            entity.ToTable("External_Professor");

            entity.Property(e => e.ExternalProfessorSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("external_professor_SSN");
            entity.Property(e => e.DeptName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("dept_name");
            entity.Property(e => e.ExternalProfessorName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("external_professor_name");
        });

        modelBuilder.Entity<ExternalProfessorPhone>(entity =>
        {
            entity.HasKey(e => new { e.PhoneNumber, e.ExternalProfessorSsn }).HasName("PK__External__2359072B2B737C0D");

            entity.ToTable("External_Professor_phone");

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("phone_number");
            entity.Property(e => e.ExternalProfessorSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("external_professor_SSN");

            entity.HasOne(d => d.ExternalProfessorSsnNavigation).WithMany(p => p.ExternalProfessorPhones)
                .HasForeignKey(d => d.ExternalProfessorSsn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_External_Professor_phone_External_Professor");
        });

        modelBuilder.Entity<InternalProfessor>(entity =>
        {
            entity.HasKey(e => e.InternalProfessorSsn).HasName("PK__Internal__251BDFA849DE7D95");

            entity.ToTable("Internal_Professor");

            entity.Property(e => e.InternalProfessorSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("Internal_professor_SSN");
            entity.Property(e => e.DeptName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("dept_name");
            entity.Property(e => e.InternalProfessorName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Internal_professor_name");
        });

        modelBuilder.Entity<InternalProfessorPhone>(entity =>
        {
            entity.HasKey(e => new { e.PhoneNumber, e.InternalProfessorSsn }).HasName("PK__Internal__33C2D790883CEC2A");

            entity.ToTable("Internal_Professor_phone");

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("phone_number");
            entity.Property(e => e.InternalProfessorSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("Internal_professor_SSN");

            entity.HasOne(d => d.InternalProfessorSsnNavigation).WithMany(p => p.InternalProfessorPhones)
                .HasForeignKey(d => d.InternalProfessorSsn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Internal_Professor_phone_Internal_Professor");
        });

        modelBuilder.Entity<Level>(entity =>
        {
            entity.HasKey(e => e.LevelId).HasName("PK__Levels__09F03C061CA2AE55");

            entity.Property(e => e.LevelId)
                .ValueGeneratedNever()
                .HasColumnName("LevelID");
            entity.Property(e => e.LevelName).HasMaxLength(20);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__project___BC799E1FF0F2C05A");

            entity.ToTable("project");

            entity.Property(e => e.ProjectId)
                .ValueGeneratedNever()
                .HasColumnName("project_id");
            entity.Property(e => e.ExternalProfessorSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("external_professor_SSN");
            entity.Property(e => e.InternalProfessorSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("internal_professor_SSN");
            entity.Property(e => e.ProjectDescription)
                .HasColumnType("text")
                .HasColumnName("Project_description");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("project_name");
            entity.Property(e => e.TeamId).HasColumnName("team_id");

            entity.HasOne(d => d.ExternalProfessorSsnNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.ExternalProfessorSsn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_project_team_External_Professor");

            entity.HasOne(d => d.InternalProfessorSsnNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.InternalProfessorSsn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_project_team_Internal_Professor");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentSsn).HasName("PK__Students__A34E12E9FC908EE4");

            entity.HasIndex(e => e.StudentId, "UQ__Students__32C52A78EC5DD016").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Students__A9D10534243C9339").IsUnique();

            entity.Property(e => e.StudentSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("StudentSSN");
            entity.Property(e => e.AcademicLevel).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Fname)
                .HasMaxLength(50)
                .HasColumnName("FName");
            entity.Property(e => e.FullName)
                .HasMaxLength(101)
                .HasComputedColumnSql("(([FName]+' ')+[LName])", false);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Gpa)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("GPA");
            entity.Property(e => e.LevelId).HasColumnName("LevelID");
            entity.Property(e => e.Lname)
                .HasMaxLength(50)
                .HasColumnName("LName");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.TeamId).HasColumnName("team_id");

            entity.HasOne(d => d.Project).WithMany(p => p.Students)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Students_project");


        });

        modelBuilder.Entity<SelectedCourse>(entity =>
        {
            entity.HasKey(e => new { e.StudentSsn, e.CourseId });
            entity.ToTable("SelectedCourses");

            entity.Property(e => e.StudentSsn)
                .HasMaxLength(14)
                .IsUnicode(false);

            entity.Property(e => e.CourseId)
                .HasMaxLength(50);

            entity.HasOne<Student>()
                .WithMany()
                .HasForeignKey(e => e.StudentSsn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SelectedCourses_Student");

            entity.HasOne<Course>()
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SelectedCourses_Course");
        });

        modelBuilder.Entity<StudentPhone>(entity =>
        {
            entity.HasKey(e => new { e.StudentSsn, e.PhoneNumber }).HasName("PK__student___2B11A60A9ECCBC2D");

            entity.ToTable("student_phone");

            entity.Property(e => e.StudentSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("StudentSSN");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);

            entity.HasOne(d => d.StudentSsnNavigation).WithMany(p => p.StudentPhones)
                .HasForeignKey(d => d.StudentSsn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_student_phone_Students");
        });



        modelBuilder.Entity<Supervisor>(entity =>
        {
            entity.HasKey(e => new { e.ProjectId, e.InternalProfessorSsn, e.ExternalProfessorSsn }).HasName("PK__supervis__FFC582F35A74EF39");

            entity.ToTable("supervisors");

            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.InternalProfessorSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("internal_professor_SSN");
            entity.Property(e => e.ExternalProfessorSsn)
                .HasMaxLength(14)
                .IsUnicode(false)
                .HasColumnName("external_professor_SSN");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("project_name");

            entity.HasOne(d => d.ExternalProfessorSsnNavigation).WithMany(p => p.Supervisors)
                .HasForeignKey(d => d.ExternalProfessorSsn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_supervisors_External_Professor");

            entity.HasOne(d => d.InternalProfessorSsnNavigation).WithMany(p => p.Supervisors)
                .HasForeignKey(d => d.InternalProfessorSsn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_supervisors_Internal_Professor");

            entity.HasOne(d => d.Project).WithMany(p => p.Supervisors)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_supervisors_project_team");
        });

        modelBuilder.Entity<TempStudentData>(entity =>
        {
            entity.HasKey(e => e.SSN);
            entity.Property(e => e.SSN).HasMaxLength(14);
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
        });
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public static implicit operator PathifyContext(AppDbContext v)
    {
        throw new NotImplementedException();
    }

}