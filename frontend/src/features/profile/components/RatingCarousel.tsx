"use client";

import { useRef } from "react";

import useHorizontalScroll from "@/hooks/useHorizontalScroll";

const RatingCarousel = ({ children }: { children: React.ReactNode }) => {
    const ref = useRef<HTMLElement>(null);
    useHorizontalScroll(ref);

    return (
        <section ref={ref} className="flex flex-shrink-0 gap-5 overflow-x-auto">
            {children}
        </section>
    );
};
export default RatingCarousel;
