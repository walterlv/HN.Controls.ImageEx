﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HN.Services;

namespace HN.Pipes
{
    public class StreamToImageSourcePipe : PipeBase<ImageSource>
    {
        public StreamToImageSourcePipe(IDesignModeService designModeService) : base(designModeService)
        {
        }

        public override async Task InvokeAsync(ILoadingContext<ImageSource> context, PipeDelegate<ImageSource> next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context.Current is Stream stream)
            {
                var tcs = new TaskCompletionSource<ImageSource>();
                await Task.Run(() =>
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        tcs.SetResult(bitmap);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                }, cancellationToken);
                context.Current = await tcs.Task;
            }

            await next(context, cancellationToken);
        }
    }
}
