# Local face models

Place the ONNX files configured in `appsettings.json` in this directory before
testing Face ID:

- `face_detection_yunet_2023mar.onnx`
- `face_recognition_sface_2021dec.onnx`

Official model pages:

- [YuNet](https://github.com/opencv/opencv_zoo/tree/main/models/face_detection_yunet)
- [SFace](https://github.com/opencv/opencv_zoo/tree/main/models/face_recognition_sface)

The API never downloads models at runtime. A relative config path is resolved
from the API content root; an absolute Windows path is also accepted. ONNX
files are ignored by Git and must be copied to the target machine during setup
or deployment.

For the later benchmark, point `SFaceModelPath` at the FP32, INT8, or INT8
block model and give each configuration a different `ModelVersion`. Changing
the recognition model or its processing version requires Students to register
their face again.

`MatchThreshold`, `MinMargin`, and `DuplicateThreshold` in `appsettings.json`
are development placeholders. Replace them with values selected from the
project benchmark before presenting final evaluation results.

Review the license information on the official model pages before distributing
model files in a company environment.
