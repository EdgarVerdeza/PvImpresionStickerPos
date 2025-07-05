using BarcodeStandard;
using SkiaSharp;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


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




        //private async void pd_Imprimeticket(object sender, PrintPageEventArgs e)
         private void pd_Imprimeticket(object sender, PrintPageEventArgs e)
        {
            System.Drawing.Graphics g = e.Graphics;

            int stickerWidth = 121;
            int stickerHeight = 94;
            int spacing = 5;
            int stickersPerRow = 3;

            System.Drawing.Font fBody1 = new System.Drawing.Font("Arial", 6, System.Drawing.FontStyle.Bold);
            System.Drawing.Font fBody2 = new System.Drawing.Font("Arial", 6, System.Drawing.FontStyle.Regular);
            System.Drawing.Font fBody3 = new System.Drawing.Font("Arial", 7, System.Drawing.FontStyle.Regular);
            System.Drawing.Font fBarcode = new System.Drawing.Font("Code 128", 12, System.Drawing.FontStyle.Regular);
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
                string precio = row["precio_publico"]?.ToString()?.Trim() ?? "0";
                string codRef = row["cod_reftitulo"]?.ToString()?.Trim() ?? "N/A";

                if (!decimal.TryParse(precio, out decimal p))
                    precio = "0";
                else
                    precio = p.ToString("N0");

                int rowNum = i / stickersPerRow;
                int colNum = i % stickersPerRow;

                int startX = colNum * (stickerWidth + spacing);
                //int startY = rowNum * (stickerHeight + spacing);
                int startY = rowNum * (stickerHeight + spacing) + 5; // +5 px margen superior

                int localY = 2;
                int lineHeight = 10;

                void DrawCenteredText(string text, System.Drawing.Font font)
                {
                    if (string.IsNullOrWhiteSpace(text))
                        text = "-";

                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
                        startX,
                        startY + localY,
                        Math.Max(stickerWidth, 10),
                        Math.Max(lineHeight, 10)
                    );

                    g.DrawString(text, font, sb, rect, sfCenter);
                    localY += lineHeight;
                }

                DrawCenteredText("BACKSIDE", fBody1);
                DrawCenteredText(nomTip, fBody2);
                DrawCenteredText("Talla " + nomTall, fBody3);
                DrawCenteredText("$" + precio, fBody2);
                DrawCenteredText("*" + codRef + "*", fBarcode);
                DrawCenteredText(codRef, fBody2);
                DrawCenteredText("HECHO EN COLOMBIA", fBody2);
            }
        }

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

            // Carga de datos ANTES de imprimir
            var slowTask = Task.Run(() => LoadDataSticker(refini, reffin, cantidad.ToString()));
            var ds = await slowTask;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para imprimir.");
                return;
            }

            dtImprimir = ds.Tables[0];

            // Imprimir
            PrintDocument pd = new PrintDocument();
            //PaperSize ps = new PaperSize("Custom", 384, 600);
            PaperSize ps = new PaperSize("Sticker", 384, 94);
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

            pd.Print();
            MessageBox.Show("Imprimio Ticket");
        }


    
    }
}
