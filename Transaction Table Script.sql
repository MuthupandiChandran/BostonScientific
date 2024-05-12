USE [MUTHU]
GO

/****** Object:  Table [dbo].[Transaction]    Script Date: 06-11-2023 09:20:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Transaction](
	[Transaction_Id] [int] IDENTITY(1,1) NOT NULL,
	[Product_Label_GTIN] [nvarchar](14) NULL,
	[Carton_Label_GTIN] [nvarchar](max) NULL,
	[DB_GTIN] [nvarchar](14) NULL,
	[WO_Lot_Num] [nvarchar](30) NULL,
	[Product_Lot_Num] [nvarchar](30) NULL,
	[Carton_Lot_Num] [nvarchar](30) NULL,
	[WO_Catalog_Num] [nvarchar](30) NULL,
	[DB_Catalog_Num] [nvarchar](30) NULL,
	[Shelf_Life] [int] NOT NULL,
	[WO_Mfg_Date] [datetime2](7) NULL,
	[Calculated_Use_By] [datetime2](7) NULL,
	[Product_Use_By] [datetime2](7) NULL,
	[Carton_Use_By] [datetime] NULL,
	[DB_Label_Spec] [nvarchar](30) NULL,
	[Product_Label_Spec] [nvarchar](30) NULL,
	[Carton_Label_Spec] [nvarchar](30) NULL,
	[DB_IFU] [nvarchar](30) NULL,
	[Scanned_IFU] [nvarchar](30) NULL,
	[User] [nvarchar](30) NULL,
	[Date_Time] [datetime2](7) NULL,
	[Rescan_Initated] [bit] NOT NULL,
	[Result] [nvarchar](10) NULL,
	[Failure_Reason] [nvarchar](30) NULL,
	[Supervisor_Name] [nvarchar](max) NULL,
	[Type] [nvarchar](max) NULL,
 CONSTRAINT [PK_Transaction] PRIMARY KEY CLUSTERED 
(
	[Transaction_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

