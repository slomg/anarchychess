import WebGL from "three/examples/jsm/capabilities/WebGL.js";
import { useEffect, useEffectEvent, useState } from "react";
import Cookies from "js-cookie";

import { useChessboardStore } from "../hooks/useChessboard";
import constants from "@/lib/constants";

const WebGLWarningAlert = () => {
    const disableDrag = useChessboardStore((x) => x.disableDrag);
    const [showAlert, setShowAlert] = useState(false);

    const showAlertEvent = useEffectEvent(() => {
        const dismissedAlert = Cookies.get(
            constants.COOKIES.DISMISSED_WEBGL_WARNING,
        );

        if (
            dismissedAlert == null &&
            !disableDrag &&
            !WebGL.isWebGL2Available()
        ) {
            setShowAlert(true);
        }
    });
    useEffect(() => {
        showAlertEvent();
    }, [disableDrag]);

    function dismissAlert() {
        setShowAlert(false);
        Cookies.set(constants.COOKIES.DISMISSED_WEBGL_WARNING, "true");
    }

    return (
        <>
            {showAlert && (
                <div
                    className="fixed top-0 left-1/2 z-50 mx-auto flex w-full
                        max-w-4xl -translate-x-1/2 items-center justify-between
                        gap-2 rounded-b-md bg-red-500 p-3 text-black"
                    data-testid="webGlWarningAlert"
                >
                    WebGL is not supported by your browser, so some animations
                    will not work.
                    <button
                        onClick={dismissAlert}
                        className="cursor-pointer text-4xl text-black
                            hover:text-black/50"
                        data-testid="closeWebGlWarningAlert"
                    >
                        ×
                    </button>
                </div>
            )}
        </>
    );
};
export default WebGLWarningAlert;
