CREATE DATABASE cfdi_db2;
GO
USE cfdi_db2;
GO

-- Tabla para la información general del comprobante
CREATE TABLE Comprobante (
    id INT IDENTITY(1,1) PRIMARY KEY,
    Version VARCHAR(10),
    Folio VARCHAR(50),
    Fecha DATETIME,
    FormaPago VARCHAR(10),
    SubTotal DECIMAL(18, 2),
    Total DECIMAL(18, 2),
    Descuento DECIMAL(18, 2),
    MetodoPago VARCHAR(10),
    Moneda VARCHAR(10),
    TipoCambio DECIMAL(18, 2),
    TipoDeComprobante VARCHAR(2),
    LugarExpedicion VARCHAR(10),
    Exportacion VARCHAR(10),
    NoCertificado VARCHAR(50),
    Certificado TEXT,
    Sello TEXT
);
GO

-- Tabla para la información del emisor
CREATE TABLE Emisor (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ComprobanteID INT,
    Nombre VARCHAR(255),
    Rfc VARCHAR(13),
    RegimenFiscal VARCHAR(10),
    FOREIGN KEY (ComprobanteID) REFERENCES Comprobante(id)
);
GO

-- Tabla para la información del receptor
CREATE TABLE Receptor (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ComprobanteID INT,
    Nombre VARCHAR(255),
    Rfc VARCHAR(13),
    UsoCFDI VARCHAR(10),
    DomicilioFiscalReceptor VARCHAR(10),
    RegimenFiscalReceptor VARCHAR(10),
    FOREIGN KEY (ComprobanteID) REFERENCES Comprobante(id)
);
GO

-- Tabla para los conceptos del comprobante
CREATE TABLE Conceptos (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ComprobanteID INT,
    ClaveProdServ VARCHAR(20),
    Cantidad DECIMAL(18, 2),
    ClaveUnidad VARCHAR(10),
    Unidad VARCHAR(50),
    NoIdentificacion VARCHAR(50),
    Descripcion VARCHAR(255),
    ValorUnitario DECIMAL(18, 2),
    Importe DECIMAL(18, 2),
    Descuento DECIMAL(18, 2),
    ObjetoImp VARCHAR(10),
    FOREIGN KEY (ComprobanteID) REFERENCES Comprobante(id)
);
GO

-- Tabla para los impuestos del comprobante
CREATE TABLE Impuestos (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ConceptoID INT,
    Impuesto VARCHAR(10),
    TipoFactor VARCHAR(10),
    TasaOCuota DECIMAL(18, 6),
    Importe DECIMAL(18, 2),
    Base DECIMAL(18, 2),
    FOREIGN KEY (ConceptoID) REFERENCES Conceptos(id)
);
GO

-- Tabla para el complemento del timbre fiscal digital
CREATE TABLE Complemento (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ComprobanteID INT,
    Version VARCHAR(10),
    UUID VARCHAR(36),
    FechaTimbrado DATETIME,
    SelloCFD TEXT,
    NoCertificadoSAT VARCHAR(50),
    SelloSAT TEXT,
    RfcProvCertif VARCHAR(13),
    FOREIGN KEY (ComprobanteID) REFERENCES Comprobante(id)
);
GO