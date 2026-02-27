create table Business(
	BusinessId int GENERATED ALWAYS AS IDENTITY primary key not null,
	BusinessName varchar(50) unique not null
)

create table Warehouse(
    WarehouseId int GENERATED ALWAYS AS IDENTITY primary key not null,
    BusinessId int not null,
    WarehouseName varchar(50) unique not null,
	CONSTRAINT fk_business
		FOREIGN KEY(BusinessId)
			REFERENCES Business(BusinessId)
)

create table Product(
    ProductId int GENERATED ALWAYS AS IDENTITY primary key not null,
    Name varchar(35) not null,
    SKU varchar(35) not null,
    Category varchar(20) not null,
    Description varchar(200) not null,
    MetricUnit varchar(20) not null,
    Status bool not null,
    Batch int,
    StockLeft int not null,
    LowStockQty int
)

create table WarehouseProduct(
    WarehouseId int not null,
    BusinessId int not null,
    ProductId int not null,
    CONSTRAINT fk_prod_warehouse 
        FOREIGN KEY(WarehouseId) 
            REFERENCES Warehouse(WarehouseId),
    CONSTRAINT fk_prod_prod 
        FOREIGN KEY(ProductId) 
            REFERENCES Product(ProductId)
)

create table Kardex(
    KardexId int GENERATED ALWAYS AS IDENTITY PRIMARY KEY not null,
    WarehousePrimaryId int not null,
    WarehouseSecondaryId int,
    BusinessId int not null,
    ProductId int not null,
    ActionType varchar(10) not null,
    ActionQty int not null,
    Reason varchar(100),
    TimeStamp timestamptz DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_wh1
        FOREIGN KEY(WarehousePrimaryId)
            REFERENCES Warehouse(WarehouseId),
    CONSTRAINT fk_wh2
        FOREIGN KEY(WarehouseSecondaryId)
            REFERENCES Warehouse(WarehouseId),
    CONSTRAINT fk_kardex_business
        FOREIGN KEY(BusinessId)
            REFERENCES Business(BusinessId),
    CONSTRAINT fk_kardex_product
        FOREIGN KEY(ProductId)
            REFERENCES Product(ProductId)
)