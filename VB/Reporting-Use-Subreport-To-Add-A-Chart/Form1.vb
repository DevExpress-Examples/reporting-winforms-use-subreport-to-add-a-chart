Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Collections.Generic
Imports DevExpress.XtraReports.UI
Imports DevExpress.DataAccess.Sql
Imports DevExpress.DataAccess.ConnectionParameters
Imports DevExpress.XtraPrinting
Imports DevExpress.XtraCharts
Imports DevExpress.XtraReports.Parameters
Imports System.IO

Namespace CreateSubreportsInCode
    Partial Public Class Form1
        Inherits Form

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            Dim mainReport As XtraReport = CreateMainReport()
            Dim designTool As New ReportDesignTool(mainReport)
            ' Comment the next line to load the generated report in Report Designer.
            'designTool.ShowRibbonDesignerDialog();
            Dim printTool As New ReportPrintTool(mainReport)
            ' Comment the next line to load the generated report in Print Preview.
            printTool.ShowRibbonPreviewDialog()
            Application.Exit()
        End Sub
        Private Function CreateMainReport() As XtraReport
            Dim report As New XtraReport() With { _
                .Bands = { _
                    New GroupHeaderBand() With { _
                        .GroupFields = { New GroupField("CategoryID") }, _
                        .Controls = { _
                            New XRLabel() With { _
                                .ExpressionBindings = { New ExpressionBinding("BeforePrint", "Text", "[CategoryName]") }, _
                                .BoundsF = New RectangleF(0,0,300,25), _
                                .Font = New Font(New FontFamily("Arial"),12,FontStyle.Bold) _
                            }, _
                            New XRLabel() With { _
                                .ExpressionBindings = { New ExpressionBinding("BeforePrint", "Text", "[Description]") }, _
                                .BoundsF = New RectangleF(0,50,300,25), _
                                .Font = New Font(New FontFamily("Arial"),9) _
                            }, _
                            New XRPictureBox() With { _
                                .ExpressionBindings = { New ExpressionBinding("BeforePrint", "ImageSource", "[Picture]") }, _
                                .BoundsF = New RectangleF(500,0,150,50), _
                                .Sizing = ImageSizeMode.ZoomImage _
                            }, _
                            New XRLabel() With { _
                                .Text = "Product Name", _
                                .BoundsF = New RectangleF(50,100,400,25), _
                                .Font = New Font(New FontFamily("Arial"),9,FontStyle.Bold) _
                            }, _
                            New XRLabel() With { _
                                .Text = "Qty Per Unit", _
                                .BoundsF = New RectangleF(450,100,100,25), _
                                .Font = New Font(New FontFamily("Arial"),9,FontStyle.Bold) _
                            }, _
                            New XRLabel() With { _
                                .Text = "Unit Price", _
                                .BoundsF = New RectangleF(550,100,100,25), _
                                .TextAlignment = TextAlignment.TopRight, _
                                .Font = New Font(New FontFamily("Arial"),9,FontStyle.Bold) _
                            } _
                        } _
                    }, _
                    New DetailBand() With { _
                        .Controls = { _
                            New XRLabel() With { _
                                .ExpressionBindings = { New ExpressionBinding("BeforePrint", "Text", "[ProductName]") }, _
                                .BoundsF = New RectangleF(50,0,400,25), _
                                .Font = New Font(New FontFamily("Arial"),9) _
                            }, _
                            New XRLabel() With { _
                                .ExpressionBindings = { New ExpressionBinding("BeforePrint", "Text", "[QuantityPerUnit]") }, _
                                .BoundsF = New RectangleF(450,0,100,25), _
                                .Font = New Font(New FontFamily("Arial"),9) _
                            }, _
                            New XRLabel() With { _
                                .ExpressionBindings = { New ExpressionBinding("BeforePrint", "Text", "[UnitPrice]") }, _
                                .BoundsF = New RectangleF(550,0,100,25), _
                                .Font = New Font(New FontFamily("Arial"),9), _
                                .TextAlignment = TextAlignment.TopRight, _
                                .TextFormatString = "{0:c2}" _
                            } _
                        }, _
                        .HeightF = 25 _
                    }, _
                    New GroupFooterBand() With { _
                        .Controls = { _
                            New XRSubreport() With { _
                                .ReportSource = CreateSubReport(), .GenerateOwnPages = True, .ParameterBindings = { New ParameterBinding("srptCategory",Nothing,"Products.CategoryID") } _
                            } _
                        }, _
                        _
                        .PageBreak = PageBreak.BeforeBand _
                    } _
                }, _
                .DataSource = CreateDataSource(), _
                .DataMember = "Products" _
            }
            Return report
        End Function
        Private Function CreateSubReport() As XtraReport
            Dim report As New XtraReport() With { _
                .Bands = { _
                    New DetailBand() With { _
                        .Controls = { _
                            New XRChart() With { _
                                .BoundsF = New RectangleF(0,0,900,650), .DataMember = "Products", .Series = { _
                                    New Series() With {.ArgumentDataMember = "ProductName"} _
                                } _
                            } _
                        } _
                    } _
                }, _
                _
                .Parameters = { _
                    New Parameter() With { _
                        .Name = "srptCategory", _
                        .Type = GetType(System.Int32) _
                    } _
                }, _
                _
                .Landscape = True, _
                .DataSource = CreateDataSource() _
            }
            Dim chart = TryCast(report.Bands(0).Controls(0), XRChart)
            chart.Parameters.Add(New XRControlParameter("chartCategory", report.Parameters(0)))
            chart.Series(0).FilterString = "CategoryID=?chartCategory"
            chart.Series(0).ValueDataMembers.AddRange(New String() { "UnitPrice"})
            Return report
        End Function
        Public Function CreateDataSource() As Object
            Dim connectionParameters As New Access97ConnectionParameters(Path.Combine(Path.GetDirectoryName(GetType(Form1).Assembly.Location), "Data/nwind.mdb"), "", "")
            Dim sqlDataSource As New SqlDataSource(connectionParameters)

            Dim queryProducts As New CustomSqlQuery() With { _
                .Name = "Products", _
                .Sql = "SELECT Products.ProductID,Products.ProductName,Products.UnitPrice,Products.QuantityPerUnit,Categories.CategoryID,Categories.CategoryName,Categories.Description,Categories.Picture FROM Products INNER JOIN Categories ON Products.CategoryID=Categories.CategoryID" _
            }
            sqlDataSource.Queries.Add(queryProducts)
            sqlDataSource.Fill()

            Return sqlDataSource
        End Function
    End Class
End Namespace
