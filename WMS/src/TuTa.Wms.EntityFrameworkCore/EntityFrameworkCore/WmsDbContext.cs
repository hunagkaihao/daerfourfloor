using Microsoft.EntityFrameworkCore;
using TuTa.Wms.AgvTasks.Aggregaes;
using TuTa.Wms.BarcodeChecks.Aggregates;
using TuTa.Wms.BarcodeLists.Aggregates;
using TuTa.Wms.Boxes.Aggregates;
using TuTa.Wms.Boxes.Entities;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.Cells.Entities;
using TuTa.Wms.ChkResultLists.Aggregates;
using TuTa.Wms.ChkResultLists.Entities;
using TuTa.Wms.Departments.Aggregates;
using TuTa.Wms.Materials.Aggregates;
using TuTa.Wms.Moves.Aggregates;
using TuTa.Wms.PickLists.Aggregates;
using TuTa.Wms.PickLists.Entities;
using TuTa.Wms.RecheckLists.Aggregates;
using TuTa.Wms.RecheckLists.Entities;
using TuTa.Wms.Skips.Aggregates;
using TuTa.Wms.StockInHistories.Aggregates;
using TuTa.Wms.StockOutHistories.Aggregates;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.Warehouses.Aggregates;
using TuTa.Wms.Warehouses.Entities;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Entities;
using ErpMaterial = TuTa.Wms.Erp.Aggregates.ErpMaterial;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace TuTa.Wms.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class WmsDbContext :
    AbpDbContext<WmsDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }


    //Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }


    #endregion

    public DbSet<Department> Departments { get; set; }

    public DbSet<Stock> Stocks { get; set; }

    public DbSet<StockInHistory> StockInHistories { get; set; }

    public DbSet<StockOutHistory> StockOutHistories { get; set; }

    public DbSet<Box> Boxes { get; set; }

    public DbSet<BoxStock> BoxStocks { get; set; }

    public DbSet<Warehouse> Warehouses { get; set; }

    public DbSet<WarehouseArea> WarehouseAreas { get; set; }

    public DbSet<Cell> Cells { get; set; }

    public DbSet<CellBox> CellBoxes { get; set; }

    public DbSet<ErpInboundOrder> ErpInboundOrders { get; set; }

    public DbSet<ErpInboundItem> ErpInboundItems { get; set; }

    public DbSet<ErpOutboundOrder> ErpOutboundOrders { get; set; }

    public DbSet<ErpOutboundItem> ErpOutboundItems { get; set; }

    public DbSet<ErpAsn> ErpAsns { get; set; }

    public DbSet<ErpMaterial> ErpMaterials { get; set; }

    public DbSet<ErpDeliveryStation> ErpDeliveryStations { get; set; }

    public DbSet<ErpDeliveryOrder> ErpOutbound { get; set; }

    public DbSet<ErpDeliveryOrderItem> ErpDeliveryOrderItems { get; set; }

    public DbSet<ErpOutboundRecord> ErpOutboundRecords { get; set; }


    public DbSet<AgvTask> AgvTasks { get; set; }

    public DbSet<BarcodeList> BarcodeLists { get; set; }

    public DbSet<Skip> Skips { get; set; }







    public WmsDbContext(DbContextOptions<WmsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(WmsConsts.DbTablePrefix + "YourEntities", WmsConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});

        builder.Entity<Department>(b =>
        {
            b.ToTable("Departments", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            
        });

        builder.Entity<Warehouse>(b =>
        {
            b.ToTable("Warehouses", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
  
         
        });

        builder.Entity<WarehouseArea>(b =>
        {
            b.ToTable("WarehouseAreas", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasOne<Warehouse>().WithMany(o => o.WarehouseAreas).HasForeignKey(o => o.WarehouseId);
        });

        builder.Entity<Cell>(b =>
        {
            b.ToTable("Cells", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
        });

        builder.Entity<CellBox>(b =>
        {
            b.ToTable("CellBoxes", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasKey(o => new { o.CellId, o.BoxId });
            b.HasOne<Cell>().WithMany(o => o.CellBoxes).HasForeignKey(o => o.CellId);
        });



        builder.Entity<Stock>(b =>
        {
            b.ToTable("Stocks", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasIndex(o => o.Id);
            //b.OwnsOne(o => o.BoxData);
            //b.OwnsOne(o => o.CellData);
            //b.OwnsOne(o => o.Warehouse);
            //b.OwnsOne(o => o.Material);
            //b.OwnsOne(o => o.ReceiveCount);
            //b.OwnsOne(o => o.CheckData);
            //b.OwnsOne(o => o.Supplier);
        });

        builder.Entity<StockInHistory>(b =>
        {
            b.ToTable("StockInHistories", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasIndex(o => o.Id);
        });

        builder.Entity<StockOutHistory>(b =>
        {
            b.ToTable("StockOutHistories", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasIndex(o => o.Id);
        });

        builder.Entity<Box>(b =>
        {
            b.ToTable("Boxes", WmsConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.OwnsOne(o => o.BoxSpecs);
            b.OwnsOne(o => o.CellData);
            b.OwnsOne(o => o.WarehouseData);
        });

        builder.Entity<BoxStock>(b =>
        {
            b.ToTable("BoxStocks", WmsConsts.DbSchema);
            b.HasKey("BoxId", "StockId");
            b.ConfigureByConvention(); //auto configure for the base class props
            b.HasOne<Box>().WithMany(o => o.StocksInBox).HasForeignKey(o => o.BoxId);
        });





        builder.Entity<ErpInboundOrder>(b =>
        {
            b.ToTable("ErpInboundOrders", WmsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(o => o.InboundOrderNo).IsUnique();
            b.HasIndex(o => o.WarehouseCode);
            b.HasIndex(o => o.PlanInboundDate);
        });

        builder.Entity<ErpInboundItem>(b =>
        {
            b.ToTable("ErpInboundItems", WmsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasOne<ErpInboundOrder>().WithMany(o => o.InboundItems).HasForeignKey(o => o.InboundOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(o => o.MaterialCode);
        });

        builder.Entity<ErpOutboundOrder>(b =>
        {
            b.ToTable("ErpOutboundOrders", WmsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(o => o.OutboundOrderNo).IsUnique();
            b.HasIndex(o => o.WarehouseCode);
            b.HasIndex(o => o.PlanOutboundDate);
        });

        builder.Entity<ErpOutboundItem>(b =>
        {
            b.ToTable("ErpOutboundItems", WmsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasOne<ErpOutboundOrder>().WithMany(o => o.OutboundItems).HasForeignKey(o => o.OutboundOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(o => o.MaterialCode);
        });

        builder.Entity<ErpDeliveryOrder>(b =>
        {
            b.ToTable("ErpDeliveryOrders", WmsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(o => o.DeliveryOrderNo).IsUnique();
        });

        builder.Entity<Material>(b =>
        {
            b.ToTable("Materials", WmsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(o => o.MaterialCode).IsUnique();
        });

        builder.Entity<ErpDeliveryStation>(b =>
        {
            b.ToTable("ErpDeliveryStations", WmsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(o => o.DeliveryCode).IsUnique();
            b.HasIndex(o => o.SyncType);
            b.HasIndex(o => o.SyncTimeStamp);
        });




        builder.Entity<AgvTask>(b =>
        {
            b.ToTable("AgvTask", WmsConsts.DbSchema);
            b.ConfigureByConvention();

        });

        builder.Entity<BarcodeList>(b =>
        {
            b.ToTable("BarcodeLists",WmsConsts.DbSchema);
            b.ConfigureByConvention();
        });
        builder.Entity<Skip>(b =>
        {
            b.ToTable("Skips", WmsConsts.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ErpOutboundRecord>(b =>
        {
            b.ToTable("erpoutbound", WmsConsts.DbSchema);
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).HasMaxLength(36).IsRequired();
            b.Property(e => e.Warehouse).HasMaxLength(50);
            b.Property(e => e.CustomerCode).HasMaxLength(50);
            b.Property(e => e.MasterId).HasMaxLength(50);
            b.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
            b.Property(e => e.QtyPerBox).HasColumnType("decimal(18,4)");
            b.Property(e => e.MaterialCode).HasMaxLength(50).IsRequired();
            b.Property(e => e.Package).HasMaxLength(50);
            b.Property(e => e.Grade).HasMaxLength(20);
            b.Property(e => e.LabelText).HasMaxLength(200);
            b.Property(e => e.DeliveryOrderNo).HasMaxLength(50);
            b.Property(e => e.ActualOutboundQuantity).HasColumnType("decimal(18,4)");
            b.HasIndex(e => new { e.DeliveryOrderNo, e.MaterialCode }).IsUnique();
        });

    }
}
