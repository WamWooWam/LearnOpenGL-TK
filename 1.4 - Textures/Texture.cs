﻿using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using System.Drawing.Imaging;
using System.Drawing;

namespace LearnOpenGL_TK
{
    //A helper class, much like Shader, meant to simplify loading textures.
    public class Texture : IDisposable
    {
        int Handle;

        //Create texture from path.
        public Texture(string path)
        {
            //Generate handle
            Handle = GL.GenTexture();


            //Bind the handle
            Use();


            //Load the image
            var image = new Bitmap(path);

            //Determine the format of the pixels. This is the order that each color channel is supplied.
            //We're loading a PNG image, which is always either RGB or RGBA.
            //However, System.Drawing loads in the opposite order of what OpenGL expects, for some reason.
            //So we use BGR or BGRA instead, to make it work properly.
            OpenTK.Graphics.OpenGL4.PixelFormat format;
            if (image.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                format = OpenTK.Graphics.OpenGL4.PixelFormat.Bgr;
            }
            else
            {
                format = OpenTK.Graphics.OpenGL4.PixelFormat.Bgra;
            }

            //Obtain bitmap data. This contains our pixels.
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);


            //Now that have our pixels, we need to set a few settings.
            //If you don't include these settings, OpenTK will refuse to draw the texture.

            //First, we set the min and mag filter. These are used for when the texture is scaled down and up, respectively.
            //Here, we use Linear for both. This means that OpenGL will try to blend pixels, meaning that textures scaled too far will look blurred.
            //You could also use (amonst other options) Nearest, which just grabs the nearest pixel, which makes the texture look pixelated if scaled too far.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);


            //Now, set the wrapping mode. S is for the X axis, and T is for the Y axis.
            //We set this to Repeat so that textures will repeat when wrapped. Not demonstrated here since the texture coordinates exactly match
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);


            //Now that our pixels have been loaded and our settings are prepared, it's time to generate a texture. We do this with GL.TexImage2D
            //Arguments:
            //  The type of texture we're generating. There are various different types of textures, but the only one we need right now is Texture2D.
            //  Level of detail. We can use this to start from a smaller mipmap (if we want), but we don't need to do that, so leave it at 0.
            //  Target format of the pixels.
            //  Width of the image
            //  Height of the image.
            //  Border of the image. This must always be 0; it's a legacy parameter that Khronos never got rid of.
            //  The format of the pixels, explained above.
            //  Data type of the pixels.
            //  And finally, the actual pixels.
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, format, PixelType.UnsignedByte, bitmapData.Scan0);


            //Free the data now that we're done with the texture
            image.UnlockBits(bitmapData);


            //Next, generate mipmaps.
            //Mipmaps are smaller copies of the texture, scaled down. Each mipmap level is half the size of the previous one
            //Generated mipmaps go all the way down to just one pixel.
            //OpenGL will automatically switch between mipmaps when an object gets sufficiently far away.
            //This prevents distant objects from having their colors become muddy, as well as saving on memory.
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        //Activate texture
        //Multiple textures can be bound, if your shader needs more than just one.
        //If you want to do that, use GL.ActiveTexture to set which slot GL.BindTexture binds to.
        //The OpenGL standard requires that there be at least 16, but there can be more depending on your graphics card.
        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        ~Texture()
        {
            GL.DeleteProgram(Handle);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
