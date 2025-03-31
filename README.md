# TinyBCSharp

[![License](https://img.shields.io/github/license/jandk/TinyBCSharp)](https://opensource.org/licenses/MIT)

## Description

TinyBCSharp is a tiny library for decoding block compressed texture formats. It's written in pure C# (.net standard 2.1),
without any dependencies, with a focus on speed and accuracy.

Currently, the following formats are supported:

- BC1 (DXT1)
- BC2 (DXT3)
- BC3 (DXT5)
- BC4 (ATI1)
- BC5 (ATI2)
- BC6H
- BC7

## Usage

Using the library is very easy, all you need is the compressed data, and the format of the data.

The following features are present:

- Partial decodes: The width and height do not need to be a multiple of the block size (4 in this case).
- Z reconstruction ("Blue" normal maps): BC5 uses two channels to store a normal map, the library can reconstruct the Z
  value from the two channels. Clamping is done to prevent negative values or values above 255.
- BC6H: BC6H is decoded to a Little-Endian Half, or Float buffer.

The library provides the `BlockDecoder` class, which can be used to decode, by specifying the format.
You can let the library create a new buffer, or pass an existing one to save allocations.

Given `src` is the compressed data, the following code snippet shows how to decode a BC1 texture.

```csharp
using TinyBCDec;

BlockDecoder decoder = BlockDecoder.create(BlockFormat.BC1);
var result = decoder.decode(256, 256, src);
```

If you want to pass an existing buffer, you can pass it as the last argument `dst`. There will be no return value.

```csharp
decoder.decode(256, 256, src, dst);
```

## Accuracy

A final note on accuracy, the library is tested against the output of DirectXTex. I generated images, encoded them,
decoded them again and compared the output. The output is identical, except when reconstructing Z in BC5, where the
library uses a different method.

This is done by doing a full float accurate implementation of BC1 (and by extension BC2 and BC3). The other formats have
bit-exact implementations. Z reconstruction uses a lookup table that is generated at runtime, and is also full float
accurate.
