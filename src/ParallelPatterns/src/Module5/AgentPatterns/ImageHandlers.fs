module ImageHandlers

open SixLabors.ImageSharp
open HelpersFSharp
open Helpers
open SixLabors.ImageSharp.PixelFormats
open System.IO


let load (imageSrc:string) = Image.Load(imageSrc)
let resize width height image = ImageHandler.Resize(image, width, height)
let convert3D image = ImageHandler.ConvertTo3D image
let setFilter filter image = ImageHandler.SetFilter(image, filter)
let saveImage (destination:string) (image:Image<Rgba32>) =
    use stream = File.Create(destination)
    image.SaveAsJpeg(stream) 