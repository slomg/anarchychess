"use client";

import { StaticImageData } from "next/image";
import Carousel from "@/components/Carousel";
import { JSX } from "react";
import clsx from "clsx";

interface GuideNestedPoint {
    title: string;
    points: GuidePoints;
}

export type GuidePoints = (string | GuideNestedPoint)[];

const GuideCard = ({
    title,
    points,
    images,
}: {
    title: string;
    points: GuidePoints;
    images: StaticImageData[];
}) => {
    const id = title.toLowerCase().replace(/\s+/g, "-");

    function renderPoints(points: GuidePoints, depth = 0): JSX.Element {
        return (
            <ul
                className={clsx(
                    "list-inside list-disc space-y-3 text-sm text-balance",
                    depth > 0 && "ml-10",
                )}
            >
                {points.map((point, i) =>
                    typeof point === "string" ? (
                        <li key={i}>{point}</li>
                    ) : (
                        <li key={i}>
                            {point.title}
                            {renderPoints(point.points, depth + 1)}
                        </li>
                    ),
                )}
            </ul>
        );
    }

    return (
        <div
            className="bg-card grid grid-rows-[auto_auto] items-center gap-5
                rounded-md p-5 md:grid-cols-[auto_min-content] md:grid-rows-1"
            id={id}
            data-testid="guideCard"
        >
            <div className="flex h-full flex-1 flex-col gap-5">
                <h1 className="text-3xl" data-testid="guideCardTitle">
                    {title}
                </h1>

                {renderPoints(points)}
            </div>

            <Carousel
                images={images}
                width={200}
                height={200}
                alt={`${title} Example`}
            />
        </div>
    );
};
export default GuideCard;
