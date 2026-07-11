/*
2026-07-11 AI-Tag
This was created with the help of Assistant, a Unity Artificial Intelligence product.
*/
using System;
using UnityEditor;
using UnityEngine;
using Unity.Cinemachine;

public class ConfigureCinemachineCamera : EditorWindow
{
    [MenuItem("Tools/Configure RPG Cinemachine Camera")]
    public static void Configure()
    {
        var camObj = GameObject.Find("CinemachineCamera");
        if (camObj == null)
        {
            Debug.LogError("GameObject 'CinemachineCamera' не найден на сцене!");
            return;
        }

        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Игрок с тегом 'Player' не найден на сцене!");
            return;
        }

        Undo.RegisterCompleteObjectUndo(camObj, "Configure Cinemachine Camera");

        // 1. Получаем или добавляем ядро виртуальной камеры
        var cmCam = camObj.GetComponent<CinemachineCamera>();
        if (cmCam == null)
        {
            cmCam = camObj.AddComponent<CinemachineCamera>();
        }

        // Назначаем цели слежения и прицеливания на игрока
        cmCam.Follow = player.transform;
        cmCam.LookAt = player.transform;

        // 2. Удаляем старый компонент простого следования CinemachineFollow
        var oldFollow = camObj.GetComponent<CinemachineFollow>();
        if (oldFollow != null)
        {
            DestroyImmediate(oldFollow);
            Debug.Log("Удален старый компонент CinemachineFollow.");
        }

        // 3. Добавляем орбитальное следование CinemachineOrbitalFollow
        var orbital = camObj.GetComponent<CinemachineOrbitalFollow>();
        if (orbital == null)
        {
            orbital = camObj.AddComponent<CinemachineOrbitalFollow>();
        }
        orbital.Radius = 5f; // Расстояние до игрока
        
        // Настраиваем углы наклона по вертикали (как в нашем старом скрипте!)
        orbital.VerticalAxis.Range = new Vector2(-20f, 70f);

        // 4. Добавляем сглаженное прицеливание CinemachineRotationComposer
        var rotationComposer = camObj.GetComponent<CinemachineRotationComposer>();
        if (rotationComposer == null)
        {
            rotationComposer = camObj.AddComponent<CinemachineRotationComposer>();
        }
        rotationComposer.Damping = new Vector2(0.5f, 0.5f); // Сглаживание

        // 5. Добавляем контроллер ввода, чтобы мышь вращала камеру
        var inputAxis = camObj.GetComponent<CinemachineInputAxisController>();
        if (inputAxis == null)
        {
            inputAxis = camObj.AddComponent<CinemachineInputAxisController>();
        }

        // Проверяем наличие CinemachineBrain на Main Camera
        var mainCam = GameObject.FindWithTag("MainCamera");
        if (mainCam != null)
        {
            var brain = mainCam.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                mainCam.AddComponent<CinemachineBrain>();
                Debug.Log("Добавлен CinemachineBrain на Main Camera.");
            }
        }

        Debug.Log("✓ Камера Cinemachine успешно настроена для RPG от третьего лица!");
    }
}
