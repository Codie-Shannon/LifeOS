from __future__ import annotations

import argparse
from pathlib import Path

from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import getSampleStyleSheet
from reportlab.lib.units import mm
from reportlab.platypus import Image, PageBreak, Paragraph, SimpleDocTemplate, Spacer


def build(group_directory: Path, output_path: Path, title: str) -> None:
    screenshots = sorted(group_directory.glob("*.png"))
    if not screenshots:
        raise SystemExit("No PNG screenshots were found in the evidence directory.")

    output_path.parent.mkdir(parents=True, exist_ok=True)
    styles = getSampleStyleSheet()
    story = [
        Paragraph(title, styles["Title"]),
        Paragraph(
            "Generated from stable screenshot evidence. Validation output and manual notes remain source-controlled beside this PDF.",
            styles["BodyText"],
        ),
        Spacer(1, 8 * mm),
    ]

    for index, screenshot in enumerate(screenshots):
        story.append(Paragraph(screenshot.stem.replace("_", " "), styles["Heading2"]))
        image = Image(str(screenshot))
        image._restrictSize(170 * mm, 220 * mm)
        story.append(image)
        if index != len(screenshots) - 1:
            story.append(PageBreak())

    document = SimpleDocTemplate(
        str(output_path),
        pagesize=A4,
        rightMargin=18 * mm,
        leftMargin=18 * mm,
        topMargin=16 * mm,
        bottomMargin=16 * mm,
        title=title,
    )
    document.build(story)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("group_directory", type=Path)
    parser.add_argument("output_path", type=Path)
    parser.add_argument("--title", default="LifeOS evidence pack")
    args = parser.parse_args()
    build(args.group_directory.resolve(), args.output_path.resolve(), args.title)


if __name__ == "__main__":
    main()
