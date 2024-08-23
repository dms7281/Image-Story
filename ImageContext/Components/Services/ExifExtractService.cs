using ExifLib;
using ImageMagick;
using Microsoft.AspNetCore.Components.Forms;

namespace ImageContext.Components.Services;

public class ExifExtractService
{
    public async Task<ImageMetadata?> ExtractExifData(IBrowserFile? imageFile)
    {
        ImageMetadata? imageMetadata = new ImageMetadata();
        
        // Read the file into a memory stream
        using var originalStream = new MemoryStream();
        await imageFile.OpenReadStream(maxAllowedSize: 1024 * 5000).CopyToAsync(originalStream);
        
        Console.WriteLine("Read file into memory stream");

        originalStream.Position = 0;

        // Stream to use for metadata extraction (may be original or converted)
        Stream processingStream = originalStream;

        // Check and handle HEIC file conversion to JPEG
        if (imageFile.ContentType == "image/heif")
        {
            // Convert HEIC to JPEG
            var convertedStream = new MemoryStream();
            await ConvertHeicToJpgAsync(originalStream, convertedStream);
            convertedStream.Position = 0;

            // Use the converted JPEG stream for metadata extraction
            processingStream = convertedStream;
        }
        
        Console.WriteLine("Preparing to read exif data");
        
        using var reader = new ExifReader(processingStream);
        
        Console.WriteLine("Reading exif data");

        if (reader.GetTagValue(ExifTags.DateTimeOriginal, out DateTime dateTaken)) imageMetadata.DateTaken = dateTaken;
        
        if (reader.GetTagValue(ExifTags.ImageDescription, out string imageDesc)) imageMetadata.ImageDescription = imageDesc;
        
        if (reader.GetTagValue(ExifTags.Artist, out string artist)) imageMetadata.Artist = artist;
        
        if (reader.GetTagValue(ExifTags.Copyright, out string copyright)) imageMetadata.Copyright = copyright;
        
        if (reader.GetTagValue(ExifTags.GPSLatitude, out double[] lat) && reader.GetTagValue(ExifTags.GPSLongitude, out double[] lng) &&
            reader.GetTagValue(ExifTags.GPSLatitudeRef, out string latRef) && reader.GetTagValue(ExifTags.GPSLongitudeRef, out string lngRef))
        {
            var latitude = ConvertGpsToDecimal(lat, latRef);
            var longitude = ConvertGpsToDecimal(lng, lngRef);
            imageMetadata.Coordinates = (latitude, longitude);
        }
        else
        {
            Console.WriteLine("Could not locate location data");
        }

        return imageMetadata;
    }
    
    private static double ConvertGpsToDecimal(double[] coordinate, string? hemisphere)
    {
        double degrees = coordinate[0];
        double minutes = coordinate[1];
        double seconds = coordinate[2];

        double decimalCoordinate = degrees + (minutes / 60) + (seconds / 3600);

        // Adjust for hemisphere (N, S, E, W)
        if (hemisphere == "S" || hemisphere == "W")
        {
            decimalCoordinate = -1 * decimalCoordinate;
        }

        return decimalCoordinate;
    }
    
    private async Task ConvertHeicToJpgAsync(Stream heicStream, Stream outputStream)
    {
        using var image = new MagickImage(heicStream);
        image.Format = MagickFormat.Jpg;
        await image.WriteAsync(outputStream);
    }
    
    public class ImageMetadata
    {
        public string? ImageDescription { get; set; }
        public DateTime DateTaken { get; set; }
        public string? Artist { get; set; }
        public string? Copyright { get; set; }
        public (double, double)? Coordinates { get; set; }
    }
}