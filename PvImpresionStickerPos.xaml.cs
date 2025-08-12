using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.Drawing.Imaging;
using BarcodeLib;
using ZXing;
using ZXing.Common;

namespace SiasoftAppExt
{
    public partial class PvImpresionStickerPos : Window
    {

        dynamic SiaWin;
        public int idemp = 0;
        string cnEmp = "";
        string cod_empresa = "";
        DataTable DtServer = new DataTable();
        DataTable dtImprimir = new DataTable();

        public PvImpresionStickerPos()
        {
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SiaWin = System.Windows.Application.Current.MainWindow;
            if (idemp <= 0) idemp = SiaWin._BusinessId;
            System.Data.DataRow foundRow = SiaWin.Empresas.Rows.Find(idemp);
            idemp = Convert.ToInt32(foundRow["BusinessId"].ToString().Trim());
            cnEmp = foundRow[SiaWin.CmpBusinessCn].ToString().Trim();
            cod_empresa = foundRow["BusinessCode"].ToString().Trim();

        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                int idr = 0; string code = ""; string name = "";
                dynamic winb = SiaWin.WindowBuscar("inmae_ref", "cod_ref", "nom_ref", "cod_ref", "idrow", "Referencias", cnEmp, false, "", idemp);
                winb.Height = 400;
                winb.Width = 400;
                winb.ShowInTaskbar = false;
                winb.Owner = Application.Current.MainWindow;
                winb.ShowDialog();
                idr = winb.IdRowReturn;
                code = winb.Codigo;
                name = winb.Nombre;
                winb = null;


                if (idr > 0)
                {
                    switch ((sender as Button).Name)
                    {
                        case "BtnSearchRefIni":
                            TxRefIni.Text = code;
                            break;
                        case "BtnSearchRefFin":
                            TxRefFin.Text = code;
                            break;
                    }
                }

            }
            catch (Exception w)
            {
                MessageBox.Show("error BtnSearch_Click:" + w);
            }
        }

        private async void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string valor = (sender as TextBox).Text;
                if (!string.IsNullOrEmpty(valor))
                {
                    string query = $"select * from inmae_ref where cod_ref='{valor}';";
                    DataTable dtDoc = await SiaWin.Func.GetDataAsync(query, idemp);
                    if (dtDoc.Rows.Count <= 0)
                    {
                        MessageBox.Show($"el codigo de referencia:{valor} no existe", "alerta", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        (sender as TextBox).Text = "";
                    }
                }

            }
            catch (Exception w)
            {
                MessageBox.Show("error TextBox_LostFocus:" + w);
            }
        }

        private DataSet LoadDataSticker(string _RefIni, string _RefFin, string _C)
        {
            try
            {
                SqlConnection con = new SqlConnection(SiaWin._cn);
                SqlCommand cmd = new SqlCommand();
                cmd.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd = new SqlCommand("_Emp_PvStickers", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ref_ini", _RefIni);
                cmd.Parameters.AddWithValue("@ref_fin", _RefFin);
                cmd.Parameters.AddWithValue("@cantidad", _C);
                cmd.Parameters.AddWithValue("@codEmp", cod_empresa);
                da = new SqlDataAdapter(cmd);
                da.SelectCommand.CommandTimeout = 0;
                da.Fill(ds);

                con.Close();
                //MessageBox.Show(ds.Tables[0].Rows.Count.ToString());
                return ds;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        private DataSet LoadData(string _RefIni, string _RefFin, string _C)
        {
            try
            {
                SqlConnection con = new SqlConnection(SiaWin._cn);
                SqlCommand cmd = new SqlCommand();
                cmd.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd = new SqlCommand("_Emp_PvStickers", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ref_ini", _RefIni);
                cmd.Parameters.AddWithValue("@ref_fin", _RefFin);
                cmd.Parameters.AddWithValue("@cantidad", _C);
                cmd.Parameters.AddWithValue("@codEmp", cod_empresa);
                da = new SqlDataAdapter(cmd);
                da.SelectCommand.CommandTimeout = 0;
                da.Fill(ds);

                con.Close();
                //MessageBox.Show(ds.Tables[0].Rows.Count.ToString());
                return ds;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }




        private void pd_Imprimeticket(object sender, PrintPageEventArgs e)
        {
            try
            {
                int TipoReporte = CmbTipoRep.SelectedIndex;
                System.Drawing.Graphics g = e.Graphics;

                int stickerWidth = 121;
                int stickerHeight = 94;
                int spacing = 5;
                int stickersPerRow = 3;
                int ajusteHorizontal = 5;
                int ajusteVertical = 4;

                System.Drawing.Font fBody1 = new System.Drawing.Font("Arial Black", 6, System.Drawing.FontStyle.Bold);
                System.Drawing.Font fBody2 = new System.Drawing.Font("Arial Black", 7, System.Drawing.FontStyle.Regular);
                System.Drawing.Font fBody3 = new System.Drawing.Font("Arial Black", 8, System.Drawing.FontStyle.Regular);
                System.Drawing.Font fBody4 = new System.Drawing.Font("Arial Black", 5, System.Drawing.FontStyle.Regular);
                System.Drawing.Font fBarcode = new System.Drawing.Font("Code39", 16, System.Drawing.FontStyle.Regular);
                System.Drawing.Font fBarcode2 = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Regular);



                System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                System.Drawing.StringFormat sfCenter = new System.Drawing.StringFormat
                {
                    Alignment = System.Drawing.StringAlignment.Center,
                    LineAlignment = System.Drawing.StringAlignment.Near
                };

                for (int i = 0; i < dtImprimir.Rows.Count; i++)
                {
                    var row = dtImprimir.Rows[i];

                    string nomTip = row["nom_tip"]?.ToString()?.Trim() ?? "N/A";
                    string nomTall = row["nom_tall"]?.ToString()?.Trim() ?? "N/A";

                    string precioMay = row["precio_mayorista"]?.ToString()?.Trim() ?? "0";
                    string precio = row["precio_publico"]?.ToString()?.Trim() ?? "0";
                    string codRef = row["cod_reftitulo"]?.ToString()?.Trim() ?? "N/A";

                    if (!decimal.TryParse(precio, out decimal p))
                        precio = "0";
                    else
                        precio = p.ToString("N0");

                    if (!decimal.TryParse(precioMay, out decimal pm))
                        precioMay = "0";
                    else
                        precioMay = pm.ToString("N0");

                    int rowNum = i / stickersPerRow;
                    int colNum = i % stickersPerRow;

                    int startX = colNum * (stickerWidth + spacing) + ajusteHorizontal; // +4 px margen horizontal
                    int startY = rowNum * (stickerHeight + spacing) + ajusteVertical; // +5 px margen superior

                    int localY = 2;
                    int lineHeight = 12;

                    void DrawCenteredText(string text, System.Drawing.Font font, int customLineHeight)
                    {
                        if (string.IsNullOrWhiteSpace(text))
                            text = "-";

                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
                            startX,
                            startY + localY,
                            Math.Max(stickerWidth, 10),
                            Math.Max(customLineHeight, 10)
                        );

                        g.DrawString(text, font, sb, rect, sfCenter);
                        localY += customLineHeight;
                    }


                    // Armado de Sticker
                    if (TipoReporte == 0)
                    {
                        // Armado de Sticker Muestra
                        DrawCenteredText(codRef, fBody2, 13);
                        DrawCenteredText(nomTip, fBody2, 13);
                        DrawCenteredText("Talla " + nomTall, fBody3, 13);
                        DrawCenteredText("$" + precioMay, fBody2, 13);
                        DrawCenteredText("$" + precio, fBody2, 13);
                        DrawCenteredText("HECHO EN COLOMBIA", fBody1, 10);
                    }
                    else
                    {
                        // Armado de Sticker Producto Terminado
                        DrawCenteredText("BACKSIDE", fBody2, 9);
                        DrawCenteredText(nomTip, fBody1, 8);
                        DrawCenteredText("Talla " + nomTall, fBody3, 12);
                        DrawCenteredText("$" + precio, fBody2, 12);

                        byte[] barcodeBytes = GenerarCodigoDeBarras(codRef, 100, 30);
                        using (var ms = new MemoryStream(barcodeBytes))
                        using (var barcodeImg = System.Drawing.Image.FromStream(ms))
                        {
                            int imgX = startX + (stickerWidth - 100) / 2;
                            g.DrawImage(barcodeImg, imgX, startY + localY, 100, 30);
                            localY += 30;
                        }

                        DrawCenteredText(codRef, fBody2, 12);
                        DrawCenteredText("HECHO EN COLOMBIA", fBody4, 8);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al Crear Ticket: ", ex.ToString());
                return;
            }

        }


        private byte[] GenerarCodigoDeBarras(string text, int width, int height)
        {
            BarcodeLib.Barcode barcode = new BarcodeLib.Barcode

            {
                IncludeLabel = false,
                Alignment = BarcodeLib.AlignmentPositions.CENTER
            };

            using (System.Drawing.Image img = barcode.Encode(
                BarcodeLib.TYPE.CODE128,
                text,
                System.Drawing.Color.Black,
                System.Drawing.Color.White,
                width,
                height))
            {
                using (var ms = new MemoryStream())
                {
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ms.ToArray();
                }
                //}


                //    using (System.Drawing.Bitmap img = barcode.Encode(
                //    BarcodeLib.TYPE.CODE128,
                //    text,
                //    System.Drawing.Color.Black,
                //    System.Drawing.Color.White,
                //    width,
                //    height))
                //{
                //    using (var ms = new MemoryStream())
                //    {
                //        img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                //        return ms.ToArray();
                //    }

            }
        }

        private void DibujarCodigoDeBarras(Graphics g, string texto, int x, int y, int ancho, int alto)
        {
            var barcode = new BarcodeLib.Barcode
            {
                IncludeLabel = false,
                Alignment = BarcodeLib.AlignmentPositions.CENTER
            };

            // Generar directamente el Bitmap
            using (Bitmap barcodeBmp = (Bitmap)barcode.Encode(
                BarcodeLib.TYPE.CODE128,
                texto,
                Color.Black,
                Color.White,
                ancho,
                alto))
            {
                g.DrawImage(barcodeBmp, x, y);
            }
        }


        private Bitmap GenerarZXingBarcode(string contenido, int ancho, int alto)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = ancho,
                    Height = alto,
                    Margin = 0,
                    PureBarcode = true
                }
            };

            return writer.Write(contenido);
        }

        //public byte[] GenerarCodigoDeBarras(string text, int width, int height)
        //{
        //    try
        //    {
        //        using (var bar = new BarcodeLib.Barcode())
        //        {
        //            bar.Alignment = BarcodeLib.AlignmentPositions.LEFT;
        //            bar.IncludeLabel = false;
        //            bar.RotateFlipType = System.Drawing.RotateFlipType.RotateNoneFlipNone;

        //            using (var bitmap = bar.Encode(BarcodeLib.TYPE.CODE39Extended, text, width, height))
        //            using (var ms = new System.IO.MemoryStream())
        //            {
        //                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        //                return ms.ToArray();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GenerarCodigoDeBarras: ", ex);
        //    }

        //}

        private async void BtnImprimir_Click(object sender, RoutedEventArgs e)
        {

            // Validaciones...
            if (string.IsNullOrEmpty(TxRefIni.Text))
            {
                MessageBox.Show("Debe llenar el campo de referencia inicial");
                return;
            }
            if (string.IsNullOrEmpty(TxRefFin.Text))
            {
                MessageBox.Show("Debe llenar el campo de referencia final");
                return;
            }

            string refini = TxRefIni.Text;
            string reffin = TxRefFin.Text;
            int cantidad = Convert.ToInt32(TxCantidad.Value);
            int TipoReporte = CmbTipoRep.SelectedIndex;

            //if (TipoReporte == 0)
            //{

            //}
            //else
            //{
                // Carga de datos ANTES de imprimir
                var slowTask = Task.Run(() => LoadDataSticker(refini, reffin, cantidad.ToString()));
                var ds = await slowTask;

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("No hay datos para imprimir.");
                    return;
                }

                dtImprimir = ds.Tables[0];

                // Cálculo dinámico de tamaño de página
                int stickersPorFila = 3;
                int totalStickers = dtImprimir.Rows.Count;
                int totalFilas = (int)Math.Ceiling(totalStickers / (double)stickersPorFila);

                int altoSticker = 94;
                int spacing = 5;
                int altoTotalPagina = totalFilas * (altoSticker + spacing);

                // Crear documento
                PrintDocument pd = new PrintDocument();

                // Asignar tamaño dinámico
                PaperSize ps = new PaperSize("Sticker", 384, altoTotalPagina);
                //PaperSize ps = new PaperSize("Sticker", 384, 94); //Ultimo
                //PaperSize ps = new PaperSize("Custom", 384, 600); //Anterior
                //pd.DefaultPageSettings.PaperSize = ps;

                // Márgenes y configuración
                pd.PrintPage += new PrintPageEventHandler(pd_Imprimeticket);
                pd.PrintController = new StandardPrintController();
                pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                pd.DefaultPageSettings.PaperSize = ps;
                pd.PrinterSettings.Copies = 1;

                //// Mostrar vista previa
                //PrintPreviewDialog previewDialog = new PrintPreviewDialog
                //{
                //    Document = pd,
                //    Width = 800,
                //    Height = 600
                //};

                //previewDialog.ShowDialog();
                // Imprimir

                pd.Print();
                MessageBox.Show("Imprimio Ticket");
            //}
        }


        //private void pd_Imprimeticket(object sender, PrintPageEventArgs e)
        //{
        //    try
        //    {
        //        System.Drawing.Graphics g = e.Graphics;

        //        int stickerWidth = 121;
        //        int stickerHeight = 94;
        //        int spacing = 5;
        //        int stickersPerRow = 3;

        //        System.Drawing.Font fBody1 = new System.Drawing.Font("Arial Black", 6, System.Drawing.FontStyle.Bold);
        //        System.Drawing.Font fBody2 = new System.Drawing.Font("Arial Black", 7, System.Drawing.FontStyle.Regular);
        //        System.Drawing.Font fBody3 = new System.Drawing.Font("Arial Black", 8, System.Drawing.FontStyle.Regular);
        //        System.Drawing.Font fBody4 = new System.Drawing.Font("Arial Black", 5, System.Drawing.FontStyle.Regular);
        //        System.Drawing.Font fBarcode = new System.Drawing.Font("Code39", 16, System.Drawing.FontStyle.Regular);
        //        System.Drawing.Font fBarcode2 = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Regular);



        //        System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
        //        System.Drawing.StringFormat sfCenter = new System.Drawing.StringFormat
        //        {
        //            Alignment = System.Drawing.StringAlignment.Center,
        //            LineAlignment = System.Drawing.StringAlignment.Near
        //        };

        //        for (int i = 0; i < dtImprimir.Rows.Count; i++)
        //        {
        //            var row = dtImprimir.Rows[i];

        //            string nomTip = row["nom_tip"]?.ToString()?.Trim() ?? "N/A";
        //            string nomTall = row["nom_tall"]?.ToString()?.Trim() ?? "N/A";
        //            string precio = row["precio_publico"]?.ToString()?.Trim() ?? "0";
        //            //string codRef = "*" + row["cod_reftitulo"]?.ToString()?.Trim() + "*";
        //            string codRef = row["cod_reftitulo"]?.ToString()?.Trim() ?? "N/A";

        //            if (!decimal.TryParse(precio, out decimal p))
        //                precio = "0";
        //            else
        //                precio = p.ToString("N0");

        //            int rowNum = i / stickersPerRow;
        //            int colNum = i % stickersPerRow;

        //            int startX = colNum * (stickerWidth + spacing);
        //            //int startY = rowNum * (stickerHeight + spacing);
        //            int startY = rowNum * (stickerHeight + spacing) + 5; // +5 px margen superior

        //            int localY = 2;
        //            int lineHeight = 12;

        //            void DrawCenteredText(string text, System.Drawing.Font font, int customLineHeight)
        //            {
        //                if (string.IsNullOrWhiteSpace(text))
        //                    text = "-";

        //                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
        //                    startX,
        //                    startY + localY,
        //                    Math.Max(stickerWidth, 10),
        //                    Math.Max(customLineHeight, 10)
        //                );

        //                g.DrawString(text, font, sb, rect, sfCenter);
        //                localY += customLineHeight;
        //                //localY += lineHeight;
        //            }
        //            DrawCenteredText("BACKSIDE", fBody2, 9);
        //            DrawCenteredText(nomTip, fBody1, 8);
        //            DrawCenteredText("Talla " + nomTall, fBody3, 12);
        //            DrawCenteredText("$" + precio, fBody2, 12);

        //            //DrawCenteredText("*" + codRef + "*", fBarcode, 20);

        //            //DrawCenteredText(codRef, fBarcode, 16);

        //            byte[] barcodeBytes = GenerarCodigoDeBarras(codRef, 110, 30);

        //            if (barcodeBytes == null || barcodeBytes.Length == 0)
        //            {
        //                throw new Exception("La imagen del código de barras no se generó correctamente.");
        //            }

        //            using (var ms = new System.IO.MemoryStream(barcodeBytes))
        //            {
        //                if (ms.Length == 0)
        //                    throw new Exception("Stream vacío: no se generó imagen válida.");

        //                using (var barcodeImg = System.Drawing.Image.FromStream(ms))
        //                {
        //                    int barcodeWidth = 100;
        //                    int barcodeHeight = 30;
        //                    int imgX = startX + (stickerWidth - barcodeWidth) / 2;
        //                    g.DrawImage(barcodeImg, imgX, startY + localY, barcodeWidth, barcodeHeight);
        //                    localY += barcodeHeight + 2;
        //                }
        //            }

        //            //using (var barcodeImg = System.Drawing.Image.FromStream(ms))
        //            //{
        //            //    int barcodeWidth = 100; // más ancho
        //            //    int barcodeHeight = 30; // más alto

        //            //    int imgX = startX + (stickerWidth - barcodeWidth) / 2; // centrado horizontal
        //            //    g.DrawImage(barcodeImg, imgX, startY + localY, barcodeWidth, barcodeHeight);
        //            //    localY += barcodeHeight + 2;
        //            //}

        //            //// 🎯 Generar código de barras con ZXing
        //            //var barcodeWriter = new BarcodeWriter
        //            //{
        //            //    Format = BarcodeFormat.CODE_128,
        //            //    Options = new EncodingOptions
        //            //    {
        //            //        Width = stickerWidth - 10,
        //            //        Height = 30,
        //            //        Margin = 0,
        //            //        PureBarcode = true
        //            //    }
        //            //};

        //            //using (var barcodeBitmap = barcodeWriter.Write(codRef))
        //            //{
        //            //    g.DrawImage(barcodeBitmap, startX + 5, startY + localY);
        //            //    localY += 35;
        //            //}
        //            //// 🎯 hasta aqui - Generar código de barras con ZXing


        //            DrawCenteredText(codRef, fBody2, 12);
        //            DrawCenteredText("HECHO EN COLOMBIA", fBody4, 8);


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error al Crear Ticket: ", ex.ToString());
        //        return;
        //    }

        //}


    }
}
