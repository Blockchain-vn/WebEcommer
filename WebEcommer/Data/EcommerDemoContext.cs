using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebEcommer.Models;

namespace WebEcommer.Data;

public partial class EcommerDemoContext : DbContext
{
    public EcommerDemoContext()
    {
    }

    public EcommerDemoContext(DbContextOptions<EcommerDemoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<HangHoa> HangHoas { get; set; }

    public virtual DbSet<Loai> Loais { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<WebEcommer.Models.GioHang> GioHangs { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    // OnConfiguring removed so DbContext uses the options provided by DI (configured in Program.cs)

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HangHoa>(entity =>
        {
            entity.HasKey(e => e.MaHh).HasName("PK_Products");

            entity.ToTable("HangHoa");

            entity.Property(e => e.MaHh).HasColumnName("MaHH").ValueGeneratedOnAdd();
            entity.Property(e => e.DonGia).HasDefaultValue(0.0);
            entity.Property(e => e.Hinh).HasMaxLength(50);
            entity.Property(e => e.MaNcc)
                .HasMaxLength(50)
                .HasColumnName("MaNCC");
            entity.Property(e => e.MoTaDonVi).HasMaxLength(50);
            entity.Property(e => e.NgaySx)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("NgaySX");
            entity.Property(e => e.TenAlias).HasMaxLength(50);
            entity.Property(e => e.TenHh)
                .HasMaxLength(50)
                .HasColumnName("TenHH");

            entity.HasOne(d => d.MaLoaiNavigation).WithMany(p => p.HangHoas)
                .HasForeignKey(d => d.MaLoai)
                .HasConstraintName("FK_Products_Categories");

            entity.HasOne(d => d.MaNccNavigation).WithMany(p => p.HangHoas)
                .HasForeignKey(d => d.MaNcc)
                .HasConstraintName("FK_Products_Suppliers");
        });

        modelBuilder.Entity<WebEcommer.Models.GioHang>(entity =>
        {
            entity.HasKey(e => e.MaGh).HasName("PK_GioHang");
            entity.ToTable("GioHang");

            entity.Property(e => e.MaGh).HasColumnName("MaGH");
            entity.Property(e => e.MaKh).HasMaxLength(20).HasColumnName("MaKH").IsRequired();
            entity.Property(e => e.MaHh).HasColumnName("MaHH");
            entity.Property(e => e.SoLuong).HasColumnName("SoLuong");
            entity.Property(e => e.NgayThem).HasDefaultValueSql("(getdate())").HasColumnType("datetime");

            entity.HasOne(e => e.HangHoa).WithMany().HasForeignKey(e => e.MaHh).HasConstraintName("FK_GioHang_HangHoa");
            entity.HasOne(e => e.KhachHang).WithMany().HasForeignKey(e => e.MaKh).HasConstraintName("FK_GioHang_KhachHang");
        });

        modelBuilder.Entity<Loai>(entity =>
        {
            entity.HasKey(e => e.MaLoai).HasName("PK_Categories");

            entity.ToTable("Loai");

            entity.Property(e => e.Hinh).HasMaxLength(50);
            entity.Property(e => e.TenLoai).HasMaxLength(50);
            entity.Property(e => e.TenLoaiAlias).HasMaxLength(50);
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNcc).HasName("PK_Suppliers");

            entity.ToTable("NhaCungCap");

            entity.Property(e => e.MaNcc)
                .HasMaxLength(50)
                .HasColumnName("MaNCC");
            entity.Property(e => e.DiaChi).HasMaxLength(50);
            entity.Property(e => e.DienThoai).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Logo).HasMaxLength(50);
            entity.Property(e => e.NguoiLienLac).HasMaxLength(50);
            entity.Property(e => e.TenCongTy).HasMaxLength(50);
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKh).HasName("PK_KhachHang");
            entity.ToTable("KhachHang");

            entity.Property(e => e.MaKh).HasMaxLength(50).IsRequired();
            entity.Property(e => e.HoTen).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DienThoai).HasMaxLength(50);
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.MatKhau).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Hinh).HasMaxLength(100);
            entity.Property(e => e.RandomKey).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
