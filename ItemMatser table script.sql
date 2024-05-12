USE [MUTHU]
GO

/****** Object:  Table [dbo].[ItemMaster]    Script Date: 07-11-2023 11:01:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ItemMaster](
	[GTIN] [nvarchar](14) NOT NULL,
	[Catalog_Num] [nvarchar](30) NULL,
	[Shelf_Life] [int] NULL,
	[Label_Spec] [nvarchar](30) NULL,
	[IFU] [nvarchar](30) NULL,
	[Edit_Date_Time] [datetime2](7) NULL,
	[Edit_By] [nvarchar](max) NULL,
	[Created] [datetime2](7) NULL,
	[Created_by] [nvarchar](max) NULL,
 CONSTRAINT [PK_ItemMaster] PRIMARY KEY CLUSTERED 
(
	[GTIN] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

