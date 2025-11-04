namespace Xmini.Tools
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    /// <summary>
    /// Hilfsklasse für einfache Bildoperationen mit System.Drawing.
    /// Diese Klasse ist bewusst einfach gehalten, damit Anfänger\*innen sie verstehen können.
    /// </summary>
    /// <remarks>
    /// Achtung: System.Drawing (System.Drawing.Common) ist auf Windows am stabilsten. Auf Linux/macOS
    /// ist zusätzliche native Bibliothek (libgdiplus) erforderlich und Microsoft empfiehlt
    /// für plattformübergreifende Server-Anwendungen Bibliotheken wie SixLabors.ImageSharp.
    /// </remarks>
    public class ImageProcessing
    {
        /// <summary>
        /// Schneidet die obere Region eines Bildes aus und gibt sie als neues Bild-Bytearray zurück.
        /// Die Breite bleibt erhalten, die Höhe wird auf <paramref name="cropHeight"/> begrenzt.
        /// Wenn das Quellbild kürzer ist als <paramref name="cropHeight"/>, wird die tatsächliche
        /// Bildhöhe verwendet (es erfolgt kein Upscaling).
        /// </summary>
        /// <param name="imageBytes">Die Bilddaten als Byte-Array (z. B. aus DB oder Upload).</param>
        /// <param name="contentType">
        /// Der MIME-Typ des Bildes (z. B. "image/png", "image/jpeg").
        /// Dieser Wert wird verwendet, um das Ausgabedateiformat zu wählen. Kann null/leer sein;
        /// dann wird JPEG als Fallback verwendet.
        /// </param>
        /// <param name="cropHeight">Gewünschte Höhe des ausgeschnittenen Bereichs in Pixel (Standard 400).</param>
        /// <returns>Byte-Array des zugeschnittenen Bildes im gewählten Ausgabeformat.</returns>
        /// <exception cref="ArgumentNullException">Wenn <paramref name="imageBytes"/> null ist.</exception>
        /// <exception cref="ArgumentException">Wenn <paramref name="imageBytes"/> leer ist oder <paramref name="cropHeight"/> &lt;= 0.</exception>
        public static byte[] CropTop(byte[] imageBytes, string contentType, int cropHeight = 400)
        {
            if (imageBytes is null)
                throw new ArgumentNullException(nameof(imageBytes));
            if (imageBytes.Length == 0)
                throw new ArgumentException("imageBytes darf nicht leer sein.", nameof(imageBytes));
            if (cropHeight <= 0)
                throw new ArgumentException("cropHeight muss größer als 0 sein.", nameof(cropHeight));

            // Lade Bild aus dem übergebenen Byte-Array
            using var inMs = new MemoryStream(imageBytes);
            using var bmp = new Bitmap(inMs);

            // Breite bleibt gleich, Höhe ist das Minimum aus gewünschter Höhe und Originalhöhe
            int width = bmp.Width;
            int height = Math.Min(cropHeight, bmp.Height);

            // Definiere das Rechteck oben links mit voller Breite und berechneter Höhe
            var cropRect = new Rectangle(0, 0, width, height);

            // Clone erzeugt eine neue Bitmap mit dem zugeschnittenen Bereich
            using var cropped = bmp.Clone(cropRect, bmp.PixelFormat);

            // Schreibe die zugeschnittene Bitmap in einen MemoryStream im gewählten Format
            using var outMs = new MemoryStream();
            var format = GetImageFormat(contentType) ?? ImageFormat.Jpeg;
            cropped.Save(outMs, format);

            // Rückgabe der Bytes des neuen Bildes
            return outMs.ToArray();
        }

        /// <summary>
        /// Hilfsmethode, die aus einem Content-Type einen passenden ImageFormat-Wert bestimmt.
        /// Gibt null zurück, wenn kein direkter Treffer vorhanden ist (FALLBACK zu JPEG möglich).
        /// </summary>
        /// <param name="contentType">MIME-Typ, z. B. "image/png".</param>
        /// <returns>Entsprechendes <see cref="ImageFormat"/> oder null, wenn unbekannt.</returns>
        private static ImageFormat GetImageFormat(string contentType)
        {
            if (string.IsNullOrEmpty(contentType)) return null;
            contentType = contentType.ToLowerInvariant();

            // WebP wird hier zu JPEG gemappt, weil System.Drawing kein WebP-Encoder hat.
            return contentType switch
            {
                "image/png" => ImageFormat.Png,
                "image/gif" => ImageFormat.Gif,
                "image/bmp" => ImageFormat.Bmp,
                "image/webp" => ImageFormat.Jpeg, // Fallback, kein native WebP-Support in System.Drawing
                "image/jpeg" or "image/jpg" => ImageFormat.Jpeg,
                _ => ImageFormat.Jpeg
            };
        }
    }
}
