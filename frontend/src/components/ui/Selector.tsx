"use client";

import React, { useMemo, useState } from "react";
import { twMerge } from "tailwind-merge";

import Button from "./Button";

type Option<T> = {
    label: React.ReactNode;
    value: T;
};

interface SelectorProps<T> {
    id?: string;
    name?: string;
    options: Option<T>[];
    value?: T;
    className?: string;
    onChange?: (e: { target: { name?: string; value: T } }) => void;
    onBlur?: React.FocusEventHandler<HTMLDivElement>;
    "data-testid"?: string;
}

const Selector = <T,>({
    id,
    name,
    options,
    value,
    className,
    onChange,
    "data-testid": testId,
}: SelectorProps<T>) => {
    const [internalIndex, setInternalIndex] = useState(0);

    const selectedIndex = useMemo(() => {
        if (value === undefined) return internalIndex;

        const idx = options.findIndex((o) => o.value === value);
        return idx === -1 ? 0 : idx;
    }, [value, options, internalIndex]);

    function select(index: number) {
        setInternalIndex(index);

        const selectedValue = options[index].value;
        onChange?.({
            target: {
                name,
                value: selectedValue,
            },
        });
    }

    return (
        <div
            id={id}
            className="flex w-full flex-wrap gap-3"
            data-testid={testId}
            data-selected={options[selectedIndex].value}
        >
            {options.map((option, i) => (
                <Button
                    key={i}
                    className={twMerge(
                        "flex-1 text-nowrap disabled:cursor-default",
                        i === selectedIndex && "border-secondary border-3",
                        className,
                    )}
                    data-testid={`selector-${option.value}`}
                    disabled={i === selectedIndex}
                    onClick={() => select(i)}
                    type="button"
                    suppressHydrationWarning
                >
                    {option.label}
                </Button>
            ))}
        </div>
    );
};

export default Selector;
