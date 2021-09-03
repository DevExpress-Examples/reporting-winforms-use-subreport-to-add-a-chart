#Region "usings"
Imports System.IO
Imports DevExpress.DataAccess.ConnectionParameters
Imports DevExpress.DataAccess.Sql
Imports DevExpress.XtraCharts
Imports DevExpress.XtraPrinting
Imports DevExpress.XtraReports.Parameters
Imports DevExpress.XtraReports.UI
#End Region
Namespace CreateSubreportsInCode
    Partial Public Class Form1
        Inherits DevExpress.XtraEditors.XtraForm

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            Dim mainReport As XtraReport = CreateMainReport()
            Dim designTool As New ReportDesignTool(mainReport)
            ' Report Designer loads the report.
            'Dim designTool As New ReportDesignTool(mainReport)
            'designTool.ShowRibbonDesignerDialog()
            Dim printTool As New ReportPrintTool(mainReport)
            ' Print Preview loads the report.
            printTool.ShowRibbonPreviewDialog()
            Application.Exit()
        End Sub
#Region "CreateMainReport"
        Private Function CreateMainReport() As XtraReport
            Dim report As New XtraReport()
            report.DataSource = CreateDataSource()
            report.DataMember = "Products"

            ' Creates a Group Header Band.
            Dim groupHBand As New GroupHeaderBand()
            groupHBand.GroupFields.Add(New GroupField("CategoryID"))
            Dim label1 As New XRLabel() With {
                .BoundsF = New RectangleF(0, 0, 300, 25),
                .Font = New Font(New FontFamily("Arial"), 12, FontStyle.Bold)
            }
            Dim label2 As New XRLabel() With {
                .BoundsF = New RectangleF(0, 50, 300, 25),
                .Font = New Font(New FontFamily("Arial"), 9)
            }
            Dim pictureBox1 As New XRPictureBox With {
                .BoundsF = New RectangleF(500, 0, 150, 50),
                .Sizing = ImageSizeMode.ZoomImage
            }
            Dim label3 As New XRLabel() With {
                .Text = "Product Name",
                .BoundsF = New RectangleF(50, 100, 400, 25),
                .Font = New Font(New FontFamily("Arial"), 9, FontStyle.Bold)
            }
            Dim label4 As New XRLabel() With {
                .Text = "Qty Per Unit",
                .BoundsF = New RectangleF(450, 100, 100, 25),
                .Font = New Font(New FontFamily("Arial"), 9, FontStyle.Bold)
            }
            Dim label5 As New XRLabel() With {
                .Text = "Unit Price",
                .BoundsF = New RectangleF(550, 100, 100, 25),
                .TextAlignment = TextAlignment.TopRight,
                .Font = New Font(New FontFamily("Arial"), 9, FontStyle.Bold)
            }
            label1.ExpressionBindings.Add(
                New ExpressionBinding("BeforePrint", "Text", "[CategoryName]"))
            label2.ExpressionBindings.Add(
                New ExpressionBinding("BeforePrint", "Text", "[Description]"))
            pictureBox1.ExpressionBindings.Add(
                New ExpressionBinding("BeforePrint", "ImageSource", "[Picture]"))
            groupHBand.Controls.AddRange(
                New XRControl() {label1, label2, pictureBox1, label3, label4, label5})

            ' Creates a Detail Band.
            Dim dtlBand As New DetailBand()
            dtlBand.HeightF = 25
            Dim label6 As New XRLabel() With {
                .BoundsF = New RectangleF(50, 0, 400, 25),
                .Font = New Font(New FontFamily("Arial"), 9)
            }
            Dim label7 As New XRLabel() With {
                .BoundsF = New RectangleF(450, 0, 100, 25),
                .Font = New Font(New FontFamily("Arial"), 9)
            }
            Dim label8 As New XRLabel() With {
                .BoundsF = New RectangleF(550, 0, 100, 25),
                .Font = New Font(New FontFamily("Arial"), 9),
                .TextAlignment = TextAlignment.TopRight,
                .TextFormatString = "{0:c2}"
            }
            label6.ExpressionBindings.Add(
                New ExpressionBinding("BeforePrint", "Text", "[ProductName]"))
            label7.ExpressionBindings.Add(
                New ExpressionBinding("BeforePrint", "Text", "[QuantityPerUnit]"))
            label8.ExpressionBindings.Add(
                New ExpressionBinding("BeforePrint", "Text", "[UnitPrice]"))
            dtlBand.Controls.AddRange(
                New XRControl() {label6, label7, label8})


            ' Creates a Group Footer Band with a Subreport.
            Dim groupFBand As New GroupFooterBand()
            groupFBand.PageBreak = PageBreak.BeforeBand
            Dim subreport1 As New XRSubreport() With {
                .ReportSource = CreateSubReport(),
                .GenerateOwnPages = True
            }
            subreport1.ParameterBindings.Add(
                New ParameterBinding("subreportCategory",
                                     Nothing,
                                     "Products.CategoryID"))
            groupFBand.Controls.Add(subreport1)

            report.Bands.AddRange(
                New Band() {groupHBand, dtlBand, groupFBand})

            Return report
        End Function
#End Region
#Region "CreateSubReport"
        Private Function CreateSubReport() As XtraReport
            Dim report As New XtraReport()
            ' Add a Detail Band with Chart.
            Dim dtlBand As New DetailBand()
            Dim chartControl1 As New XRChart() With {
                .BoundsF = New RectangleF(0, 0, 900, 650),
                .DataMember = "Products"
            }
            chartControl1.Series.Add(
                New Series() With {
                .ArgumentDataMember = "ProductName"})
            dtlBand.Controls.Add(chartControl1)
            report.Bands.Add(dtlBand)
            ' Add a parameter.
            report.Parameters.Add(New Parameter() With {
                                  .Name = "subreportCategory",
                                  .Type = GetType(System.Int32)
                                  })

            report.Landscape = True
            report.DataSource = CreateDataSource()
            ' Configure the chart.
            chartControl1.Parameters.Add(New XRControlParameter("chartCategory", report.Parameters(0)))
            chartControl1.Series(0).FilterString = "CategoryID=?chartCategory"
            chartControl1.Series(0).ValueDataMembers.AddRange(New String() {"UnitPrice"})

            Return report
        End Function
#End Region

        Public Function CreateDataSource() As Object
            Dim connectionParameters As New SQLiteConnectionParameters("Data\\nwind.db", "")
            Dim sqlDataSource As New SqlDataSource(connectionParameters)

            Dim queryProducts As New CustomSqlQuery() With {
                .Name = "Products",
                .Sql = "SELECT " +
                "Products.ProductID,Products.ProductName,Products.UnitPrice,Products.QuantityPerUnit," +
                "Categories.CategoryID,Categories.CategoryName,Categories.Description,Categories.Picture " +
                "FROM Products INNER JOIN Categories ON Products.CategoryID=Categories.CategoryID"}
            sqlDataSource.Queries.Add(queryProducts)
            sqlDataSource.Fill()

            Return sqlDataSource
        End Function
    End Class
End Namespace
