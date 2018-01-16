using System.Web.Mvc;
using QRCoder;
using System;

namespace Users.Infrastructure
{
    public static class QRcodeHelper
    {

        public static MvcHtmlString GenerateQRCode(this HtmlHelper htmlHelper, string url)
        {
            if(string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode code = new PngByteQRCode(data);
            byte[] qrCodeAsPngByteArr = code.GetGraphic(20);

            var img = new TagBuilder("img");
            img.Attributes.Add("src", string.Format("data:image/png;base64,{0}", Convert.ToBase64String(qrCodeAsPngByteArr)));
            img.Attributes.Add("width", "120");
            img.Attributes.Add("height", "120");
            return MvcHtmlString.Create(img.ToString(TagRenderMode.SelfClosing));
        }
    }
}