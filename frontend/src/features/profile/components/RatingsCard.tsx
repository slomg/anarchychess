"use client";

import { useEffect, useEffectEvent, useState } from "react";
import dynamic from "next/dynamic";
import clsx from "clsx";

import TimeControlIcon from "@/features/lobby/components/TimeControlIcon";
import { RatingOverview } from "@/lib/apiClient";
import constants from "@/lib/constants";
import Card from "@/components/ui/Card";

const Chart = dynamic(() => import("react-apexcharts"), { ssr: false });

interface DataPoint {
    x: number;
    y: number;
}

const RatingCard = ({ overview }: { overview: RatingOverview }) => {
    const {
        timeControl,
        ratings,
        highest: highestRating,
        lowest: lowestRating,
        current: currentRating,
    } = overview;

    const [flatRatings, setFlatRatings] = useState<DataPoint[]>([]);

    const generateRatings = useEffectEvent(() => {
        // if there are enough recent rating points, use them directly
        if (ratings.length >= 2) {
            setFlatRatings(
                ratings.map(({ achievedAt, rating }) => ({
                    x: new Date(achievedAt).valueOf(),
                    y: rating,
                })),
            );
            return;
        }

        const now = Date.now();
        const monthAgo = now - 1000 * 60 * 60 * 24 * 30;
        setFlatRatings([
            { x: monthAgo, y: currentRating },
            { x: now, y: currentRating },
        ]);
    });
    useEffect(() => generateRatings(), []);

    const earliestRating = ratings.at(0)?.rating ?? currentRating;
    const ratingChange = currentRating - earliestRating;

    function formatNumberWithSign(num: number): string {
        if (num === 0) return "±0";
        return num > 0 ? `+${num}` : `${num}`;
    }

    function getRatingChangeColor(): string {
        if (ratingChange === 0) return "text-neutral-400";
        return ratingChange > 0 ? "text-green-400" : "text-red-400";
    }

    return (
        <Card
            className="min-w-96"
            data-testid={`ratingCard-${overview.timeControl}`}
        >
            <section className="flex justify-between">
                <span className="flex gap-2">
                    {constants.TIME_CONTROL_LABELS[timeControl]}
                    <TimeControlIcon
                        className="h-6 w-6"
                        timeControl={timeControl}
                    />
                </span>
                {currentRating}
            </section>

            <Chart
                options={{
                    chart: {
                        type: "line",
                        background: "#0F0C14",
                        sparkline: {
                            enabled: true,
                        },
                        zoom: {
                            enabled: false,
                        },
                        toolbar: {
                            show: false,
                        },
                    },
                    xaxis: {
                        labels: {
                            show: false,
                        },
                        crosshairs: {
                            show: false,
                        },
                    },
                    yaxis: {
                        labels: {
                            show: false,
                        },
                    },
                    fill: {
                        type: "solid",
                        colors: ["#B8ABCE"],
                    },
                    tooltip: {
                        theme: "dark",
                        x: {
                            formatter: (val) =>
                                new Date(val).toLocaleString("en-GB", {
                                    day: "2-digit",
                                    month: "2-digit",
                                    year: "2-digit",
                                    hour: "2-digit",
                                    minute: "2-digit",
                                    hour12: true,
                                }),
                        },
                    },
                }}
                series={[
                    {
                        name: "Rating",
                        data: flatRatings,
                    },
                ]}
                height="100"
            />

            <section
                className="grid grid-cols-[1fr_min-content]"
                data-testid="ratingInfoSection"
            >
                <span>Current</span>
                <span data-testid="currentRating">{currentRating}</span>
                <span>Height</span>
                <span
                    className="text-end text-green-400"
                    data-testid="maxRating"
                >
                    {highestRating}
                </span>
                <span>Lowest</span>
                <span className="text-end text-red-400" data-testid="minRating">
                    {lowestRating}
                </span>
                <span>
                    Rating Change
                    <span className="text-text/50 ms-2">last month</span>
                </span>
                <span
                    className={clsx(getRatingChangeColor(), "text-end")}
                    data-testid="ratingChange"
                >
                    {formatNumberWithSign(ratingChange)}
                </span>
            </section>
        </Card>
    );
};
export default RatingCard;
