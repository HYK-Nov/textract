# 📖 Textract

사용자가 지정한 이미지 영역에서 텍스트를 인식하는 WPF 기반 OCR 도구입니다.  
Tesseract OCR 엔진과 OpenCV를 활용해 한글, 영어, 일본어 등 다양한 언어의 문자를 정확히 추출할 수 있습니다.

---

## 📌 주요 기능

- 📁 이미지 로딩 (JPG, PNG 등 지원)
- 🖱️ 마우스로 **OCR 대상 영역 선택**
- 🔍 **확대/축소 지원** (마우스 휠)
- 🧠 **Tesseract OCR** 기반 문자 인식
- 🌐 **다국어 지원** (`eng`, `jpn`, 등)
- 📝 결과 텍스트 화면 출력 및 복사 가능

## 🛠️ 기술 스택

| 구성 요소       | 사용 기술 |
|----------------|-----------|
| Desktop UI     | WPF (.NET 9) |
| Language       | C# |
| OCR Engine     | [Tesseract 4.1](https://github.com/tesseract-ocr/tessdata) |
| 이미지 처리     | OpenCvSharp |
| 패키지 관리     | NuGet |

## 🖥️ 실행 화면

|OCR 결과 |
|-----------|
|![Image](https://github.com/user-attachments/assets/a8866cec-02e8-4c0a-a193-651a832b1121)|

## 🚀 실행 방법

> [Release 다운로드](https://github.com/HYK-Nov/textract/releases/tag/1.0.0)