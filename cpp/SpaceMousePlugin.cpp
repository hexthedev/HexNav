#include <cstring>
#include <iostream>

// Include 3DConnexion headers
#include "ConnexionClient.h"
#include "ConnexionClientAPI.h"

#if defined(__APPLE__)
    #define UNITY_INTERFACE_EXPORT __attribute__((visibility("default")))
#else
    #define UNITY_INTERFACE_EXPORT __declspec(dllexport)
#endif

// Global variables to store Space Mouse state
static int16_t gTx, gTy, gTz, gRx, gRy, gRz;
static uint32_t gButtons;

// Callback function for Space Mouse events
static void HandleSpaceMouseEvent(unsigned int productID, unsigned int messageType, void *messageArgument)
{
    switch (messageType)
    {
        case kConnexionMsgDeviceState:
        {
            ConnexionDeviceState *state = static_cast<ConnexionDeviceState*>(messageArgument);
            gTx = state->axis[0];
            gTy = state->axis[1];
            gTz = state->axis[2];
            gRx = state->axis[3];
            gRy = state->axis[4];
            gRz = state->axis[5];
            gButtons = state->buttons;
            break;
        }
        default:
            break;
    }
}

extern "C" {
    UNITY_INTERFACE_EXPORT void InitializeSpaceMouse() {
        SetConnexionHandlers(HandleSpaceMouseEvent, nullptr, nullptr, true);
        uint16_t clientID = RegisterConnexionClient(kConnexionClientWildcard, nullptr, kConnexionClientModeTakeOver, kConnexionMaskAll);
        if (clientID == 0) {
            std::cerr << "Failed to register Connexion client" << std::endl;
        }
    }

    UNITY_INTERFACE_EXPORT void ShutdownSpaceMouse() {
        UnregisterConnexionClient(0);
        CleanupConnexionHandlers();
    }

    UNITY_INTERFACE_EXPORT void GetSpaceMouseState(int16_t* tx, int16_t* ty, int16_t* tz, 
                                                   int16_t* rx, int16_t* ry, int16_t* rz, 
                                                   uint32_t* buttons) {
        *tx = gTx;
        *ty = gTy;
        *tz = gTz;
        *rx = gRx;
        *ry = gRy;
        *rz = gRz;
        *buttons = gButtons;
    }

    // Unity-specific function to initialize the plugin
    UNITY_INTERFACE_EXPORT void UnityPluginLoad() {
        InitializeSpaceMouse();
    }

    // Unity-specific function to unload the plugin
    UNITY_INTERFACE_EXPORT void UnityPluginUnload() {
        ShutdownSpaceMouse();
    }
}