// Copyright (c) .NET Foundation and Contributors.  All Rights Reserved.  See LICENSE in the project root for license information.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using TorchSharp;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.nn.functional;

namespace TorchSharp.Examples
{
    /// <summary>
    /// Modified version of original AlexNet to fix CIFAR10 32x32 images.
    /// </summary>
    public class AlexNet : Module
    {
        private readonly Module features;
        private readonly Module avgPool;
        private readonly Module classifier;

        public AlexNet(string name, int numClasses, Device device = null) : base(name)
        {
            features = Sequential(
                ("c1", Conv2d(3, 64, kernelSize: 3, stride: 2, padding: 1)),
                ("r1", ReLU(inPlace: true)),
                ("mp1", MaxPool2d(kernelSize: new long[] { 2, 2 })),
                ("c2", Conv2d(64, 192, kernelSize: 3, padding: 1)),
                ("r2", ReLU(inPlace: true)),
                ("mp2", MaxPool2d(kernelSize: new long[] { 2, 2 })),
                ("c3", Conv2d(192, 384, kernelSize: 3, padding: 1)),
                ("r3", ReLU(inPlace: true)),
                ("c4", Conv2d(384, 256, kernelSize: 3, padding: 1)),
                ("r4", ReLU(inPlace: true)),
                ("c5", Conv2d(256, 256, kernelSize: 3, padding: 1)),
                ("r5", ReLU(inPlace: true)),
                ("mp3", MaxPool2d(kernelSize: new long[] { 2, 2 })));

            avgPool = AdaptiveAvgPool2d(new long[] { 2, 2 });

            classifier = Sequential(
                ("d1", Dropout()),
                ("l1", Linear(256 * 2 * 2, 4096)),
                ("r1", ReLU(inPlace: true)),
                ("d2", Dropout()),
                ("l2", Linear(4096, 4096)),
                ("r3", ReLU(inPlace: true)),
                ("d3", Dropout()),
                ("l3", Linear(4096, numClasses))
            );

            RegisterComponents();

            if (device != null && device.type == DeviceType.CUDA)
                this.to(device);
        }

        public override Tensor forward(Tensor input)
        {
            var f = features.forward(input);
            var avg = avgPool.forward(f);

            var x = avg.view(new long[] { avg.shape[0], 256 * 2 * 2 });

            return classifier.forward(x);
        }
    }

}
