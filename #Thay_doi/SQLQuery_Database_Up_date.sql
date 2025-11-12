-- ===================================
-- 1. TẠO DATABASE VÀ SỬ DỤNG
-- (Lưu ý: Nếu DB đã tồn tại, lệnh này sẽ báo lỗi trừ khi bạn xóa thủ công)
-- ===================================
CREATE DATABASE PCShopDBtest;
GO
USE PCShopDBtest;
GO

-- ===============================
-- 2. Bảng Khách hàng
-- ===============================
CREATE TABLE Customers (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Phone NVARCHAR(20),
    Address NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- ===============================
-- 3. Bảng Danh mục sản phẩm (Tối ưu hóa: ComponentType, IsRequiredForBuild)
-- ===============================
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    ComponentType NVARCHAR(100) UNIQUE NOT NULL,    -- Key cố định (VD: CPU, Mainboard) dùng trong logic build PC
    IsRequiredForBuild BIT DEFAULT 0,               -- Đánh dấu linh kiện bắt buộc (1=Bắt buộc)
    ParentCategoryID INT NULL,
    Description NVARCHAR(255),
    FOREIGN KEY (ParentCategoryID) REFERENCES Categories(CategoryID)
);
GO

-- ===============================
-- 4. Bảng Sản phẩm (Linh kiện)
-- ===============================
CREATE TABLE Products (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryID INT NOT NULL,
    Brand NVARCHAR(100) NOT NULL,
    ProductName NVARCHAR(150) NOT NULL,
    Model NVARCHAR(100),
    Specifications NVARCHAR(MAX),
    Price DECIMAL(18,2) NOT NULL,
    Stock INT DEFAULT 0,
    ImageURL NVARCHAR(255),
    WarrantyMonths INT DEFAULT 12,
    ReleaseDate DATE,
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);
GO

-- ===============================
-- 5. Bảng Bộ PC (PCBuild) - Cấu hình sẵn
-- ===============================
CREATE TABLE PCBuild (
    PCBuildID INT IDENTITY(1,1) PRIMARY KEY,
    PCBuildName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(MAX),
    TotalPrice DECIMAL(18,2) DEFAULT 0,
    ImageURL NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO

-- ===============================
-- 6. Bảng Chi tiết Bộ PC (Tối ưu hóa: Thêm ComponentType)
-- ===============================
CREATE TABLE PCBuildDetails (
    PCBuildDetailID INT IDENTITY(1,1) PRIMARY KEY,
    PCBuildID INT NOT NULL,
    ProductID INT NOT NULL,
    ComponentType NVARCHAR(50) NOT NULL, -- Vai trò của sản phẩm (VD: CPU, RAM_SLOT_1)
    Quantity INT DEFAULT 1,
    FOREIGN KEY (PCBuildID) REFERENCES PCBuild(PCBuildID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
GO

-- ===============================
-- 7. Bảng PC Tùy chỉnh (CustomPC)
-- ===============================
CREATE TABLE CustomPC (
    CustomPCID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT NOT NULL,
    BuildName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(MAX),
    TotalPrice DECIMAL(18,2) DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
);
GO

-- ===============================
-- 8. Bảng Chi tiết PC Tùy chỉnh (Tối ưu hóa: Thêm ComponentType)
-- ===============================
CREATE TABLE CustomPCDetails (
    CustomPCDetailID INT IDENTITY(1,1) PRIMARY KEY,
    CustomPCID INT NOT NULL,
    ProductID INT NOT NULL,
    ComponentType NVARCHAR(50) NOT NULL, -- Vai trò của sản phẩm
    Quantity INT DEFAULT 1,
    Note NVARCHAR(255),
    FOREIGN KEY (CustomPCID) REFERENCES CustomPC(CustomPCID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
GO

-- ===============================
-- 9. Bảng Giỏ hàng (Tối ưu hóa: Tách thành Carts và CartItems)
-- ===============================
CREATE TABLE Carts (
    CartID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT NOT NULL UNIQUE,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
);

CREATE TABLE CartItems (
    CartItemID INT IDENTITY(1,1) PRIMARY KEY,
    CartID INT NOT NULL,
    ItemType NVARCHAR(50) NOT NULL,     -- 'PRODUCT', 'PCBUILD', 'CUSTOMPC'
    ItemID INT NOT NULL,                -- ID tương ứng
    Quantity INT DEFAULT 1,
    AddedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CartID) REFERENCES Carts(CartID) ON DELETE CASCADE
);
GO

-- ===============================
-- 10. Bảng Trạng thái Đơn hàng
-- ===============================
CREATE TABLE OrderStatus (
    StatusID INT IDENTITY(1,1) PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255)
);
GO

-- ===============================
-- 11. Bảng Đơn hàng (Tối ưu hóa: Thêm CurrentStatusID)
-- ===============================
CREATE TABLE Orders (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT NOT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) DEFAULT 0,
    CurrentStatusID INT NOT NULL, -- Trạng thái hiện tại
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    FOREIGN KEY (CurrentStatusID) REFERENCES OrderStatus(StatusID)
);
GO

-- ===============================
-- 12. Chi tiết đơn hàng
-- ===============================
CREATE TABLE OrderDetails (
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT DEFAULT 1,
    UnitPrice DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
GO

-- ===============================
-- 13. Lịch sử trạng thái đơn hàng
-- ===============================
CREATE TABLE OrderStatusHistory (
    HistoryID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    StatusID INT NOT NULL,
    UpdatedAt DATETIME DEFAULT GETDATE(),
    Note NVARCHAR(255),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID) ON DELETE CASCADE,
    FOREIGN KEY (StatusID) REFERENCES OrderStatus(StatusID)
);
GO

-- ===============================
-- 14. Bảng Thanh toán
-- ===============================
CREATE TABLE Payments (
    PaymentID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentMethod NVARCHAR(50),
    PaymentDate DATETIME DEFAULT GETDATE(),
    Note NVARCHAR(255),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID) ON DELETE CASCADE
);
GO

CREATE TABLE ProductAttributes (
    AttributeID INT IDENTITY(1,1) PRIMARY KEY,
    ProductID INT NOT NULL,
    AttributeName NVARCHAR(100) NOT NULL, -- Tên thuộc tính (VD: 'Chipset', 'Socket', 'Dung lượng')
    AttributeValue NVARCHAR(255) NOT NULL, -- Giá trị thuộc tính (VD: 'Z790', 'LGA1700', '16GB')
    
    -- Tạo khóa ngoại liên kết tới bảng Products
    -- ON DELETE CASCADE nghĩa là nếu bạn xóa 1 sản phẩm, tất cả thuộc tính của nó cũng tự động bị xóa.
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE CASCADE
);
GO