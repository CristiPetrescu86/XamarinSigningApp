using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinLicentaApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PDFSignPage : ContentPage
    {
        public PDFSignPage()
        {
            InitializeComponent();
        }

        private string path;

        private async void ButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // pickMultipleAsync pentru mai tarziu
                var file = await FilePicker.PickAsync(
                    new PickOptions
                    {
                        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                            { DevicePlatform.UWP, new [] {".pdf",".xml"} }
                        })
                    });


                if (file == null)
                {
                    LabelInfo.Text = "Nu e bun";
                }
                else
                {
                    LabelInfo.Text = file.FullPath;
                    path = file.FullPath;
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ButtonClicked2(object sender, EventArgs e)
        {
            /*
            //load a pdf document
            PdfDocument doc = new PdfDocument();
            doc.LoadFromFile(@path);

            //load the certificate
            PdfCertificate cert = new PdfCertificate(@"C:\Users\cpetr\Downloads\cert.pfx", null);

            //initialize a PdfSignature instance
            PdfSignature signature = new PdfSignature(doc, doc.Pages[3], cert, "Signature1");

            //set the signature location
            signature.Bounds = new RectangleF(new PointF(200, 200), new SizeF(200, 90));

            //set the image of signature
            //signature.SignImageSource = PdfImage.FromFile(@"C:\Users\Administrator\Desktop\signImage.png");

            //set the content of signature
            signature.GraphicsMode = GraphicMode.SignImageAndSignDetail;
            signature.NameLabel = "Signer:";
            signature.Name = "Gary";
            signature.ContactInfoLabel = "ContactInfo:";
            signature.ContactInfo = signature.Certificate.GetNameInfo(System.Security.Cryptography.X509Certificates.X509NameType.SimpleName, true);
            signature.DistinguishedNameLabel = "DN: ";
            signature.DistinguishedName = signature.Certificate.IssuerName.Name;
            signature.LocationInfoLabel = "Location:";
            signature.LocationInfo = "Chengdu";
            signature.ReasonLabel = "Reason: ";
            signature.Reason = "Le document est certifie";
            signature.DateLabel = "Date:";
            signature.Date = DateTime.Now;
            signature.DocumentPermissions = PdfCertificationFlags.AllowFormFill | PdfCertificationFlags.ForbidChanges;
            signature.Certificated = true;

            //set fonts
            signature.SignDetailsFont = new PdfFont(PdfFontFamily.TimesRoman, 10f);
            signature.SignNameFont = new PdfFont(PdfFontFamily.Courier, 15);

            //set the sign image layout mode
            signature.SignImageLayout = SignImageLayout.None;

            //save the file
            doc.SaveToFile("signature.pdf");
            */
        }
    }
}