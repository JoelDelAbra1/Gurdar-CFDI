using System;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Xml;

class Program
{
    static void Main()
    {
        string connectionString = "Server=(local);Database=cfdi_db2;Integrated Security=true; Encrypt=False"; // Reemplaza con tu cadena de conexión

        string projectDirectory = @"C:\Proyecto Integrador";
        string compressedDirectory = Path.Combine(projectDirectory, "Comprimidos");
        string noGuardadosDirectory = Path.Combine(projectDirectory, "NoGuardados");
        string guardadosDirectory = Path.Combine(projectDirectory, "Guardados");

        VerifyAndCreateDirectory(compressedDirectory);
        VerifyAndCreateDirectory(noGuardadosDirectory);
        VerifyAndCreateDirectory(guardadosDirectory);

        Console.WriteLine("Directorios verificados y creados si era necesario.");

        // Step 1: Descomprimir los archivos .zip y mover los .xml a NoGuardados
        ProcessCompressedFiles(compressedDirectory, noGuardadosDirectory);

        // Step 2: Procesar los archivos .xml y guardarlos en la base de datos
        ProcessXmlFiles(noGuardadosDirectory, guardadosDirectory, connectionString);

        Console.WriteLine("Proceso completado.");
    }
    static void VerifyAndCreateDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    static void ProcessCompressedFiles(string compressedDirectory, string noGuardadosDirectory)
    {
        foreach (string zipFilePath in Directory.GetFiles(compressedDirectory, "*.zip"))
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        string destinationPath = Path.Combine(noGuardadosDirectory, entry.FullName);
                        entry.ExtractToFile(destinationPath, true);
                    }
                }
            }
            // Eliminar el archivo .zip después de descomprimir
            File.Delete(zipFilePath);
        }
    }

    static void ProcessXmlFiles(string noGuardadosDirectory, string guardadosDirectory, string connectionString)
    {
        foreach (string xmlFilePath in Directory.GetFiles(noGuardadosDirectory, "*.xml"))
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath);

            // Extract data from XML and save to database
            SaveXmlDataToDatabase(doc, connectionString);

            // Mover el archivo procesado a Guardados con fecha y hora en el nombre
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string destinationPath = Path.Combine(guardadosDirectory, $"{Path.GetFileNameWithoutExtension(xmlFilePath)}_{timestamp}{Path.GetExtension(xmlFilePath)}");
            File.Move(xmlFilePath, destinationPath);
        }
    }

    static void SaveXmlDataToDatabase(XmlDocument doc, string connectionString)
    {
        XmlNamespaceManager nsmgr = GetNamespaceManager(doc);

        XmlNode comprobanteNode = doc.SelectSingleNode("/cfdi:Comprobante", nsmgr);
        if (comprobanteNode != null)
        {
            string version = comprobanteNode.Attributes["Version"]?.InnerText;
            string folio = comprobanteNode.Attributes["Folio"]?.InnerText;
            DateTime? fecha = comprobanteNode.Attributes["Fecha"] != null ? (DateTime?)DateTime.Parse(comprobanteNode.Attributes["Fecha"].InnerText) : null;
            string formaPago = comprobanteNode.Attributes["FormaPago"]?.InnerText;
            decimal? subTotal = comprobanteNode.Attributes["SubTotal"] != null ? (decimal?)decimal.Parse(comprobanteNode.Attributes["SubTotal"].InnerText) : null;
            decimal? total = comprobanteNode.Attributes["Total"] != null ? (decimal?)decimal.Parse(comprobanteNode.Attributes["Total"].InnerText) : null;
            decimal? descuento = comprobanteNode.Attributes["Descuento"] != null ? (decimal?)decimal.Parse(comprobanteNode.Attributes["Descuento"].InnerText) : null;
            string metodoPago = comprobanteNode.Attributes["MetodoPago"]?.InnerText;
            string moneda = comprobanteNode.Attributes["Moneda"]?.InnerText;
            decimal? tipoCambio = comprobanteNode.Attributes["TipoCambio"] != null ? (decimal?)decimal.Parse(comprobanteNode.Attributes["TipoCambio"].InnerText) : null;
            string tipoDeComprobante = comprobanteNode.Attributes["TipoDeComprobante"]?.InnerText;
            string lugarExpedicion = comprobanteNode.Attributes["LugarExpedicion"]?.InnerText;
            string exportacion = comprobanteNode.Attributes["Exportacion"]?.InnerText;
            string noCertificado = comprobanteNode.Attributes["NoCertificado"]?.InnerText;
            string certificado = comprobanteNode.Attributes["Certificado"]?.InnerText;
            string sello = comprobanteNode.Attributes["Sello"]?.InnerText;

            int comprobanteId;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Comprobante (Version, Folio, Fecha, FormaPago, SubTotal, Total, Descuento, MetodoPago, Moneda, TipoCambio, TipoDeComprobante, LugarExpedicion, Exportacion, NoCertificado, Certificado, Sello)
                                 OUTPUT INSERTED.id
                                 VALUES (@Version, @Folio, @Fecha, @FormaPago, @SubTotal, @Total, @Descuento, @MetodoPago, @Moneda, @TipoCambio, @TipoDeComprobante, @LugarExpedicion, @Exportacion, @NoCertificado, @Certificado, @Sello)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Version", (object)version ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Folio", (object)folio ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Fecha", (object)fecha ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FormaPago", (object)formaPago ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SubTotal", (object)subTotal ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Total", (object)total ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Descuento", (object)descuento ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MetodoPago", (object)metodoPago ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Moneda", (object)moneda ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TipoCambio", (object)tipoCambio ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TipoDeComprobante", (object)tipoDeComprobante ?? DBNull.Value);
                    command.Parameters.AddWithValue("@LugarExpedicion", (object)lugarExpedicion ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Exportacion", (object)exportacion ?? DBNull.Value);
                    command.Parameters.AddWithValue("@NoCertificado", (object)noCertificado ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Certificado", (object)certificado ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Sello", (object)sello ?? DBNull.Value);

                    comprobanteId = (int)command.ExecuteScalar();
                }

                // Parse Emisor
                XmlNode emisorNode = comprobanteNode.SelectSingleNode("cfdi:Emisor", nsmgr);
                if (emisorNode != null)
                {
                    string emisorNombre = emisorNode.Attributes["Nombre"]?.InnerText;
                    string emisorRfc = emisorNode.Attributes["Rfc"]?.InnerText;
                    string emisorRegimenFiscal = emisorNode.Attributes["RegimenFiscal"]?.InnerText;

                    string emisorQuery = @"INSERT INTO Emisor (ComprobanteID, Nombre, Rfc, RegimenFiscal)
                                           VALUES (@ComprobanteID, @Nombre, @Rfc, @RegimenFiscal)";

                    using (SqlCommand emisorCommand = new SqlCommand(emisorQuery, connection))
                    {
                        emisorCommand.Parameters.AddWithValue("@ComprobanteID", comprobanteId);
                        emisorCommand.Parameters.AddWithValue("@Nombre", (object)emisorNombre ?? DBNull.Value);
                        emisorCommand.Parameters.AddWithValue("@Rfc", (object)emisorRfc ?? DBNull.Value);
                        emisorCommand.Parameters.AddWithValue("@RegimenFiscal", (object)emisorRegimenFiscal ?? DBNull.Value);

                        emisorCommand.ExecuteNonQuery();
                    }
                }

                // Parse Receptor
                XmlNode receptorNode = comprobanteNode.SelectSingleNode("cfdi:Receptor", nsmgr);
                if (receptorNode != null)
                {
                    string receptorNombre = receptorNode.Attributes["Nombre"]?.InnerText;
                    string receptorRfc = receptorNode.Attributes["Rfc"]?.InnerText;
                    string receptorUsoCFDI = receptorNode.Attributes["UsoCFDI"]?.InnerText;
                    string receptorDomicilioFiscal = receptorNode.Attributes["DomicilioFiscalReceptor"]?.InnerText;
                    string receptorRegimenFiscal = receptorNode.Attributes["RegimenFiscalReceptor"]?.InnerText;

                    string receptorQuery = @"INSERT INTO Receptor (ComprobanteID, Nombre, Rfc, UsoCFDI, DomicilioFiscalReceptor, RegimenFiscalReceptor)
                                             VALUES (@ComprobanteID, @Nombre, @Rfc, @UsoCFDI, @DomicilioFiscalReceptor, @RegimenFiscalReceptor)";

                    using (SqlCommand receptorCommand = new SqlCommand(receptorQuery, connection))
                    {
                        receptorCommand.Parameters.AddWithValue("@ComprobanteID", comprobanteId);
                        receptorCommand.Parameters.AddWithValue("@Nombre", (object)receptorNombre ?? DBNull.Value);
                        receptorCommand.Parameters.AddWithValue("@Rfc", (object)receptorRfc ?? DBNull.Value);
                        receptorCommand.Parameters.AddWithValue("@UsoCFDI", (object)receptorUsoCFDI ?? DBNull.Value);
                        receptorCommand.Parameters.AddWithValue("@DomicilioFiscalReceptor", (object)receptorDomicilioFiscal ?? DBNull.Value);
                        receptorCommand.Parameters.AddWithValue("@RegimenFiscalReceptor", (object)receptorRegimenFiscal ?? DBNull.Value);

                        receptorCommand.ExecuteNonQuery();
                    }
                }

                // Parse Conceptos
                XmlNodeList conceptosNodes = comprobanteNode.SelectNodes("cfdi:Conceptos/cfdi:Concepto", nsmgr);
                foreach (XmlNode conceptoNode in conceptosNodes)
                {
                    string claveProdServ = conceptoNode.Attributes["ClaveProdServ"]?.InnerText;
                    decimal? cantidad = conceptoNode.Attributes["Cantidad"] != null ? (decimal?)decimal.Parse(conceptoNode.Attributes["Cantidad"].InnerText) : null;
                    string claveUnidad = conceptoNode.Attributes["ClaveUnidad"]?.InnerText;
                    string unidad = conceptoNode.Attributes["Unidad"]?.InnerText;
                    string noIdentificacion = conceptoNode.Attributes["NoIdentificacion"]?.InnerText;
                    string descripcion = conceptoNode.Attributes["Descripcion"]?.InnerText;
                    decimal? valorUnitario = conceptoNode.Attributes["ValorUnitario"] != null ? (decimal?)decimal.Parse(conceptoNode.Attributes["ValorUnitario"].InnerText) : null;
                    decimal? importe = conceptoNode.Attributes["Importe"] != null ? (decimal?)decimal.Parse(conceptoNode.Attributes["Importe"].InnerText) : null;
                    decimal? descuentoConcepto = conceptoNode.Attributes["Descuento"] != null ? (decimal?)decimal.Parse(conceptoNode.Attributes["Descuento"].InnerText) : null;
                    string objetoImp = conceptoNode.Attributes["ObjetoImp"]?.InnerText;

                    int conceptoId;

                    string conceptoQuery = @"INSERT INTO Conceptos (ComprobanteID, ClaveProdServ, Cantidad, ClaveUnidad, Unidad, NoIdentificacion, Descripcion, ValorUnitario, Importe, Descuento, ObjetoImp)
                                            OUTPUT INSERTED.id
                                            VALUES (@ComprobanteID, @ClaveProdServ, @Cantidad, @ClaveUnidad, @Unidad, @NoIdentificacion, @Descripcion, @ValorUnitario, @Importe, @Descuento, @ObjetoImp)";

                    using (SqlCommand conceptoCommand = new SqlCommand(conceptoQuery, connection))
                    {
                        conceptoCommand.Parameters.AddWithValue("@ComprobanteID", comprobanteId);
                        conceptoCommand.Parameters.AddWithValue("@ClaveProdServ", (object)claveProdServ ?? DBNull.Value);
                        conceptoCommand.Parameters.AddWithValue("@Cantidad", (object)cantidad ?? DBNull.Value);
                        conceptoCommand.Parameters.AddWithValue("@ClaveUnidad", (object)claveUnidad ?? DBNull.Value);
                        conceptoCommand.Parameters.AddWithValue("@Unidad", (object)unidad ?? DBNull.Value);
                        conceptoCommand.Parameters.AddWithValue("@NoIdentificacion", (object)noIdentificacion ?? DBNull.Value);
                        conceptoCommand.Parameters.AddWithValue("@Descripcion", (object)descripcion ?? DBNull.Value);
                        conceptoCommand.Parameters.AddWithValue("@ValorUnitario", (object)valorUnitario ?? DBNull.Value);
                        conceptoCommand.Parameters.AddWithValue("@Importe", (object)importe ?? DBNull.Value);
                        conceptoCommand.Parameters.AddWithValue("@Descuento", (object)descuentoConcepto ?? DBNull.Value);
                        conceptoCommand.Parameters.AddWithValue("@ObjetoImp", (object)objetoImp ?? DBNull.Value);

                        conceptoId = (int)conceptoCommand.ExecuteScalar();
                    }

                    // Parse Impuestos
                    XmlNodeList trasladosNodes = conceptoNode.SelectNodes("cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado", nsmgr);
                    foreach (XmlNode trasladoNode in trasladosNodes)
                    {
                        string impuesto = trasladoNode.Attributes["Impuesto"]?.InnerText;
                        string tipoFactor = trasladoNode.Attributes["TipoFactor"]?.InnerText;
                        decimal? tasaOCuota = trasladoNode.Attributes["TasaOCuota"] != null ? (decimal?)decimal.Parse(trasladoNode.Attributes["TasaOCuota"].InnerText) : null;
                        decimal? importeImpuesto = trasladoNode.Attributes["Importe"] != null ? (decimal?)decimal.Parse(trasladoNode.Attributes["Importe"].InnerText) : null;
                        decimal? baseImpuesto = trasladoNode.Attributes["Base"] != null ? (decimal?)decimal.Parse(trasladoNode.Attributes["Base"].InnerText) : null;

                        string impuestoQuery = @"INSERT INTO Impuestos (ConceptoID, Impuesto, TipoFactor, TasaOCuota, Importe, Base)
                                                VALUES (@ConceptoID, @Impuesto, @TipoFactor, @TasaOCuota, @Importe, @Base)";

                        using (SqlCommand impuestoCommand = new SqlCommand(impuestoQuery, connection))
                        {
                            impuestoCommand.Parameters.AddWithValue("@ConceptoID", conceptoId);
                            impuestoCommand.Parameters.AddWithValue("@Impuesto", (object)impuesto ?? DBNull.Value);
                            impuestoCommand.Parameters.AddWithValue("@TipoFactor", (object)tipoFactor ?? DBNull.Value);
                            impuestoCommand.Parameters.AddWithValue("@TasaOCuota", (object)tasaOCuota ?? DBNull.Value);
                            impuestoCommand.Parameters.AddWithValue("@Importe", (object)importeImpuesto ?? DBNull.Value);
                            impuestoCommand.Parameters.AddWithValue("@Base", (object)baseImpuesto ?? DBNull.Value);

                            impuestoCommand.ExecuteNonQuery();
                        }
                    }
                }

                // Parse Complemento
                XmlNode complementoNode = comprobanteNode.SelectSingleNode("cfdi:Complemento/tfd:TimbreFiscalDigital", nsmgr);
                if (complementoNode != null)
                {
                    string complementoVersion = complementoNode.Attributes["Version"]?.InnerText;
                    string uuid = complementoNode.Attributes["UUID"]?.InnerText;
                    DateTime? fechaTimbrado = complementoNode.Attributes["FechaTimbrado"] != null ? (DateTime?)DateTime.Parse(complementoNode.Attributes["FechaTimbrado"].InnerText) : null;
                    string selloCFD = complementoNode.Attributes["SelloCFD"]?.InnerText;
                    string noCertificadoSAT = complementoNode.Attributes["NoCertificadoSAT"]?.InnerText;
                    string selloSAT = complementoNode.Attributes["SelloSAT"]?.InnerText;
                    string rfcProvCertif = complementoNode.Attributes["RfcProvCertif"]?.InnerText;

                    string complementoQuery = @"INSERT INTO Complemento (ComprobanteID, Version, UUID, FechaTimbrado, SelloCFD, NoCertificadoSAT, SelloSAT, RfcProvCertif)
                                               VALUES (@ComprobanteID, @Version, @UUID, @FechaTimbrado, @SelloCFD, @NoCertificadoSAT, @SelloSAT, @RfcProvCertif)";

                    using (SqlCommand complementoCommand = new SqlCommand(complementoQuery, connection))
                    {
                        complementoCommand.Parameters.AddWithValue("@ComprobanteID", comprobanteId);
                        complementoCommand.Parameters.AddWithValue("@Version", (object)complementoVersion ?? DBNull.Value);
                        complementoCommand.Parameters.AddWithValue("@UUID", (object)uuid ?? DBNull.Value);
                        complementoCommand.Parameters.AddWithValue("@FechaTimbrado", (object)fechaTimbrado ?? DBNull.Value);
                        complementoCommand.Parameters.AddWithValue("@SelloCFD", (object)selloCFD ?? DBNull.Value);
                        complementoCommand.Parameters.AddWithValue("@NoCertificadoSAT", (object)noCertificadoSAT ?? DBNull.Value);
                        complementoCommand.Parameters.AddWithValue("@SelloSAT", (object)selloSAT ?? DBNull.Value);
                        complementoCommand.Parameters.AddWithValue("@RfcProvCertif", (object)rfcProvCertif ?? DBNull.Value);

                        complementoCommand.ExecuteNonQuery();
                    }
                }
            }
        }
    }

    private static XmlNamespaceManager GetNamespaceManager(XmlDocument doc)
    {
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");
        nsmgr.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");
        return nsmgr;
    }
}
