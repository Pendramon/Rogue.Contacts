using Microsoft.EntityFrameworkCore;
using Rogue.Contacts.Data.Model;
using Rogue.Contacts.Shared;

namespace Rogue.Contacts.Data;

public class ContactsContext : DbContext
{
    public ContactsContext(DbContextOptions<ContactsContext> options)
        : base(options)
    {
    }

    public DbSet<Party> Parties => Set<Party>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<OrganizationRole> OrganizationRoles => Set<OrganizationRole>();

    public DbSet<OrganizationPermission> OrganizationPermissions => Set<OrganizationPermission>();

    public DbSet<OrganizationRolePermission> OrganizationRolePermissions => Set<OrganizationRolePermission>();

    public DbSet<UserOrganizationRole> UserOrganizationRoles => Set<UserOrganizationRole>();

    public DbSet<Business> Businesses => Set<Business>();

    public DbSet<BusinessRole> BusinessRoles => Set<BusinessRole>();

    public DbSet<BusinessPermission> BusinessPermissions => Set<BusinessPermission>();

    public DbSet<BusinessRolePermission> BusinessRolePermissions => Set<BusinessRolePermission>();

    public DbSet<UserBusinessRole> UserBusinessRoles => Set<UserBusinessRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Party>(p =>
        {
            p.UseTpcMappingStrategy();
            p.Property(p => p.Name).IsRequired().HasMaxLength(40);
            p.HasIndex(p => p.Name).IsUnique();
            p.Property(u => u.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<User>(u =>
        {
            u.ToTable("users");
            u.Property(u => u.DisplayName).IsRequired().HasMaxLength(40);
            u.Property(u => u.Email).IsRequired().HasMaxLength(254);
            u.HasIndex(u => u.Email).IsUnique();
            u.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
        });

        modelBuilder.Entity<Organization>(o =>
        {
            o.ToTable("organizations");
        });

        modelBuilder.Entity<OrganizationRole>(or =>
        {
            or.ToTable("organization_roles");
            or.HasKey(r => r.Id);
            or.Property(r => r.Id).IsRequired().ValueGeneratedOnAdd().UseIdentityColumn();
            or.Property(r => r.Name).IsRequired().HasMaxLength(32);
            or.HasOne(or => or.Organization).WithMany(b => b.OrganizationRoles).HasForeignKey(or => or.Id);
        });

        modelBuilder.Entity<OrganizationPermission>(op =>
        {
            op.ToTable("organization_permissions");
            op.HasKey(op => op.Id);
            op.Property(op => op.Id).IsRequired().ValueGeneratedNever();
            op.Property(op => op.Name).IsRequired();
            op.HasData(Enum.GetValues<OrganizationPermissionEnum>().Select(p => (OrganizationPermission)p));
        });

        modelBuilder.Entity<OrganizationRolePermission>(orp =>
        {
            orp.ToTable("organization_role_permissions");
            orp.HasKey(orp => new { orp.OrganizationRoleId, orp.OrganizationPermissionId });
            orp.HasOne(orp => orp.OrganizationRole).WithMany(or => or.OrganizationRolePermissions).HasForeignKey(orp => orp.OrganizationRoleId);
            orp.HasOne(orp => orp.OrganizationPermission).WithMany(op => op.RolePermissions).HasForeignKey(orp => orp.OrganizationPermissionId);
        });

        modelBuilder.Entity<UserOrganizationRole>(uor =>
        {
            uor.ToTable("user_organization_roles");
            uor.HasKey(uor => new { uor.UserId, uor.OrganizationRoleId });
            uor.HasOne(uor => uor.User).WithMany(u => u.UserOrganizationRoles).HasForeignKey(uor => uor.UserId);
            uor.HasOne(uor => uor.OrganizationRole).WithMany(or => or.UserOrganizationRoles).HasForeignKey(uor => uor.OrganizationRoleId);
        });

        modelBuilder.Entity<Business>(b =>
        {
            b.ToTable("businesses");
            b.HasKey(b => b.Id);
            b.Property(b => b.Id).IsRequired().ValueGeneratedOnAdd().UseIdentityColumn();
            b.Property(b => b.Name).IsRequired().HasMaxLength(40);
            b.Property(b => b.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<BusinessRole>(br =>
        {
            br.ToTable("business_roles");
            br.HasKey(br => br.Id);
            br.Property(br => br.Id).IsRequired().ValueGeneratedOnAdd().UseIdentityColumn();
            br.Property(br => br.Name).IsRequired().HasMaxLength(32);
            br.HasOne(br => br.Business).WithMany(b => b.BusinessRoles).HasForeignKey(br => br.Id);
        });

        modelBuilder.Entity<BusinessPermission>(bp =>
        {
            bp.ToTable("business_permissions");
            bp.HasKey(bp => bp.Id);
            bp.Property(bp => bp.Id).IsRequired().ValueGeneratedNever();
            bp.Property(bp => bp.Name).IsRequired();
            bp.HasData(Enum.GetValues<BusinessPermissionEnum>().Select(p => (BusinessPermission)p));
        });

        modelBuilder.Entity<BusinessRolePermission>(brp =>
        {
            brp.ToTable("business_role_permissions");
            brp.HasKey(brp => new { brp.BusinessRoleId, brp.BusinessPermissionId });
            brp.HasOne(brp => brp.BusinessRole).WithMany(br => br.BusinessRolePermissions).HasForeignKey(brp => brp.BusinessRoleId);
            brp.HasOne(brp => brp.BusinessPermission).WithMany(bp => bp.RolePermissions).HasForeignKey(brp => brp.BusinessPermissionId);
        });

        modelBuilder.Entity<UserBusinessRole>(ubr =>
        {
            ubr.ToTable("user_business_roles");
            ubr.HasKey(ubr => new { ubr.UserId, ubr.BusinessRoleId });
            ubr.HasOne(ubr => ubr.User).WithMany(u => u.UserBusinessRoles).HasForeignKey(ubr => ubr.UserId);
            ubr.HasOne(ubr => ubr.BusinessRole).WithMany(br => br.UserBusinessRoles).HasForeignKey(ubr => ubr.BusinessRoleId);
        });
    }
}
