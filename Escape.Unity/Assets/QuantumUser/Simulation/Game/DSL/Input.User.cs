namespace Quantum {
using Photon.Deterministic;
using static Photon.Deterministic.FP;
using static Photon.Deterministic.FPVector2;
public unsafe partial struct Input {
  public FP InterpolationAlpha {
    get => (FP)InterpolationAlphaEncoded / 255;
    set => InterpolationAlphaEncoded = (byte)FPMath.Clamp(value * 255, 0, 255).AsInt;
  }

  public FPVector2 MovementDirection {
    get => DecodeAnyDirection(MovementDirectionEncoded);
    set => MovementDirectionEncoded = EncodeAnyDirection(value);
  }

  public FP MovementMagnitude {
    get => DecodeMagnitude(MovementMagnitudeEncoded);
    set => MovementMagnitudeEncoded = EncodeMagnitude(value);
  }

  byte EncodeAnyDirection(FPVector2 direction) {
    if (direction == default) {
      return 0;
    }

    FP angle = RadiansSigned(Up, direction) * Rad2Deg;
    angle = (angle + 360) % 360 / 2 + 1;
    return (byte)angle.AsInt;
  }

  FPVector2 DecodeAnyDirection(byte directionEncoded) {
    if (directionEncoded == 0) {
      return Zero;
    }

    int angle = (directionEncoded - 1) * 2;
    return Rotate(Up, angle * Deg2Rad);
  }

  static FP DecodeMagnitude(byte encodedMagnitude) =>
          (FP)(int)encodedMagnitude / 255 * QConstants.INPUT_MAX_MAGNITUDE;

  static byte EncodeMagnitude(FP magnitude) =>
          (byte)(FPMath.InverseLerp(0, QConstants.INPUT_MAX_MAGNITUDE, magnitude) * 255).AsInt;

}
}