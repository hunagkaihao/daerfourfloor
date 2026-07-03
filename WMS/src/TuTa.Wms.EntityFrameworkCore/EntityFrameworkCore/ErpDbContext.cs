using Microsoft.EntityFrameworkCore;
using TuTa.Wms;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.Erp.Aggregates;
using ErpMaterial = TuTa.Wms.Erp.Aggregates.ErpMaterial;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace TuTa.Wms.EntityFrameworkCore;

[ConnectionStringName("Default")]
public class ErpDbContext :
    AbpDbContext<ErpDbContext>
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */


    public ErpDbContext(DbContextOptions<ErpDbContext> options)
        : base(options)
    {

    }

    public DbSet<ErpStockAftChk> ErpStockAftChks { get; set; }
    public DbSet<ErpMaterial> ErpMaterials { get; set; }
    public DbSet<ErpPickOrder> ErpPickOrders { get; set; }  
    public DbSet<ErpRecheckNotifier> ErpRecheckNotifiers { get; set; }
    public DbSet<ErpStockInReturn> ErpStockInReturns { get; set; }
    public DbSet<ErpStockOutReturn> ErpStockOutReturns { get; set; }
    public DbSet<ErpStockMoveReturn> ErpStockMoveReturns { get; set; }
    public DbSet<ErpStockCheck> ErpStockChecks { get; set; }
    public DbSet<ErpDepartment> ErpDepartments { get; set; }
    public DbSet<ErpNoPlanPickType> ErpNoPlanPickTypes { get; set; }    
    public DbSet<ErpPickMan> ErpPickMen { get; set; }
    public DbSet<ErpStateChgNotifier> ErpStateChgNotifiers { get; set; }    
    public DbSet<ErpBarcode> ErpBarcodes { get; set; }

    public DbSet<ErpMove> ErpMoves { get; set; }
    public DbSet<ErpDeptType> ErpDeptTypes { get; set; }
    public DbSet<ErpDeptTypeDetail> ErpDeptTypeDetails { get; set; }
    public DbSet<ErpWarehouseAreaPrdt> erpWarehouseAreaPrdts { get; set; }
    public DbSet<ErpWorkstationMaterialRequest> ErpWorkstationMaterialRequests { get; set; }
            public DbSet<ErpWorkstationMaterialReceipt> ErpWorkstationMaterialReceipts { get; set; }
        public DbSet<ErpWorkshopMaterialTransfer> ErpWorkshopMaterialTransfers { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(WmsConsts.DbTablePrefix + "YourEntities", WmsConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});

        builder.Entity<ErpStockAftChk>(b =>
        {
            b.ToTable("CLBY_CGQCTWO", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasKey(b => b.DHTZD_TXM);
            //b.OwnsOne(o => o.TypeInfo, t =>
            //{
            //    t.Property(i => i.TypeCode).HasColumnName("TypeCode");
            //    t.Property(i => i.TypeName).HasColumnName("TypeName");
            //});
        });

        builder.Entity<ErpMaterial>(b =>
        {
            b.ToTable("INI_PRDT", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasKey(b => b.Id);
            //b.OwnsOne(o => o.TypeInfo, t =>
            //{
            //    t.Property(i => i.TypeCode).HasColumnName("TypeCode");
            //    t.Property(i => i.TypeName).HasColumnName("TypeName");
            //});
        });

        builder.Entity<ErpPickOrder>(b =>
        {
            b.ToTable("CLCK_CHKDTZDTWO", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasKey(b => new { b.CHKTZD_ITM });
            //b.OwnsOne(o => o.TypeInfo, t =>
            //{
            //    t.Property(i => i.TypeCode).HasColumnName("TypeCode");
            //    t.Property(i => i.TypeName).HasColumnName("TypeName");
            //});
        });

        builder.Entity<ErpRecheckNotifier>(b =>
        {
            b.ToTable("CLBY_CGFQTWO", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasKey(b => new { b.CKFQTZD_ID, b.DHTZD_TXM });
            //b.OwnsOne(o => o.TypeInfo, t =>
            //{
            //    t.Property(i => i.TypeCode).HasColumnName("TypeCode");
            //    t.Property(i => i.TypeName).HasColumnName("TypeName");
            //});
        });

        builder.Entity<ErpStockInReturn>(b =>
        {
            b.ToTable("CLCK_RKDTWO", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasKey(b => new { b.Id });
            //b.OwnsOne(o => o.TypeInfo, t =>
            //{
            //    t.Property(i => i.TypeCode).HasColumnName("TypeCode");
            //    t.Property(i => i.TypeName).HasColumnName("TypeName");
            //});
        });

        builder.Entity<ErpStockOutReturn>(b =>
        {
            b.ToTable("CLCK_CHKDTWO", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasKey(b => new { b.Id });
            //b.OwnsOne(o => o.TypeInfo, t =>
            //{
            //    t.Property(i => i.TypeCode).HasColumnName("TypeCode");
            //    t.Property(i => i.TypeName).HasColumnName("TypeName");
            //});
        });

        builder.Entity<ErpStockMoveReturn>(b =>
        {
            b.ToTable("CLCK_ZCDBDTWO", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasKey(b => new { b.Id });
            //b.OwnsOne(o => o.TypeInfo, t =>
            //{
            //    t.Property(i => i.TypeCode).HasColumnName("TypeCode");
            //    t.Property(i => i.TypeName).HasColumnName("TypeName");
            //});
        });

        builder.Entity<ErpStockCheck>(b =>
        {
            b.ToTable("CLCK_JNMX", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasKey(b => new { b.Id });
        });

        builder.Entity<ErpDepartment>(b =>
        {
            b.ToTable("VDFDZ_DEPT", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasKey(b => new { b.DEPT_ID });
            //b.OwnsOne(o => o.TypeInfo, t =>
            //{
            //    t.Property(i => i.TypeCode).HasColumnName("TypeCode");
            //    t.Property(i => i.TypeName).HasColumnName("TypeName");
            //});
        });

        builder.Entity<ErpNoPlanPickType>(b =>
        {
            b.ToTable("VWJHCKCHK_TYPE", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasKey(b => new { b.CHKTYPE_ID });
            //b.OwnsOne(o => o.TypeInfo, t =>
            //{
            //    t.Property(i => i.TypeCode).HasColumnName("TypeCode");
            //    t.Property(i => i.TypeName).HasColumnName("TypeName");
            //});
        });

        builder.Entity<ErpPickMan>(b =>
        {
            b.ToTable("VDFDZ_MAN", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasKey(b => new { b.MAN_NAME });
            //b.OwnsOne(o => o.TypeInfo, t =>
            //{
            //    t.Property(i => i.TypeCode).HasColumnName("TypeCode");
            //    t.Property(i => i.TypeName).HasColumnName("TypeName");
            //});
        });

        builder.Entity<ErpStateChgNotifier>(b =>
        {
            b.ToTable("CLCK_KCZTCHANG", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasKey(b => new { b.CKZTCHANG_ID });
            //b.OwnsOne(o => o.TypeInfo, t =>
            //{
            //    t.Property(i => i.TypeCode).HasColumnName("TypeCode");
            //    t.Property(i => i.TypeName).HasColumnName("TypeName");
            //}); 
        });

        builder.Entity<ErpBarcode>(b =>
        {
            b.ToTable("CLBY_DHTZD", WmsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasKey(b => new { b.DHTZD_TXM });
        });

        builder.Entity<ErpMove>(b =>
        {
            b.ToTable("CLCK_ZCKDBTZD", WmsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasKey(b => new { b.ZCDBD_ID });
        });

        builder.Entity<ErpDeptType>(b =>
        {
            b.ToTable("CLCK_CJCHKLB", WmsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasNoKey();
        });

        builder.Entity<ErpDeptTypeDetail>(b =>
        {
            b.ToTable("VCLCK_CJCHKLBMX", WmsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasNoKey();
        });


        builder.Entity<ErpWarehouseAreaPrdt>(b =>
        {
            b.ToTable("VCLCK_CKAREALBMX", WmsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasKey(b => new { b.ID });
        });

        builder.Entity<ErpWorkstationMaterialRequest>(b =>
        {
            b.ToTable("ERP_WORKSTATION_MATERIAL_REQUEST", WmsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasKey(b => b.Id);
            
            // 配置索引
            b.HasIndex(b => b.SortingBatch).IsUnique();
            b.HasIndex(b => b.DeliveryPointLocation);
            b.HasIndex(b => b.Status);
            b.HasIndex(b => b.DeliveryTime);
        });

        builder.Entity<ErpWorkstationMaterialReceipt>(b =>
        {
            b.ToTable("ERP_WORKSTATION_MATERIAL_RECEIPT", WmsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasKey(b => b.Id);
            
            // 配置索引
            b.HasIndex(b => b.SortingBatch).IsUnique();
            b.HasIndex(b => b.ReceiptTime);
                           b.HasIndex(b => b.CreationTime);
           });

           builder.Entity<ErpWorkshopMaterialTransfer>(b =>
           {
               b.ToTable("ERP_WORKSHOP_MATERIAL_TRANSFER", WmsConsts.DbSchema);
               b.ConfigureByConvention();
               b.HasKey(b => b.Id);
               b.HasIndex(b => b.StartLocation);
               b.HasIndex(b => b.EndLocation);
               b.HasIndex(b => b.Status);
               b.HasIndex(b => b.CreationTime);
           });
    }
}
