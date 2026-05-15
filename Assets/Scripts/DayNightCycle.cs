using UnityEngine;
using TMPro;
using System.Collections;

namespace itsmakingthings_daynightcycle
{
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Time Settings")]
        public float startTimeOfDay = 12f;
        public float minutesPerDay = 40f;
        public float TimeOfDay => _timeOfDay;
        private float _timeOfDay;
        private bool _isTimeRunning = true;

        [Header("Reset Settings")]
        public float resetSpeed = 1f;
        public bool forwardsOnly = false;
        private Coroutine _timeResetCoroutine;

        [Header("Directional Light Settings")]
        public Light sunLight;
        public Light moonLight;
        public Transform rotationPivot;
        public Transform rotationPivotMoon;
        [Range(0, 259)] public float rotationOffsetY = 0f;
        public Camera sceneCamera;

        [Header("Moon Offset Settings")]
        [Tooltip("Moon offset angle at sunrise (when moon leads the sun)")]
        [Range(120f, 180f)] public float moonOffsetMorning = 170f;

        [Tooltip("Moon offset angle at sunset (when moon trails the sun)")]
        [Range(180f, 240f)] public float moonOffsetEvening = 190f;

        [Tooltip("Time when moon offset transitions from morning to evening (e.g. 6→18h)")]
        public Vector2 moonOffsetTransitionHours = new(6f, 18f);

        [Header("UI Settings")]
        public TextMeshProUGUI timeText;

        [Header("Fog Control")]
        public bool enableFogControl = true;

        [Header("Water Settings")]
        public Renderer waterRenderer;
        public string waterColorProperty = "_WaterColor";
        private MaterialPropertyBlock _waterPropertyBlock;

        [System.Serializable]
        public class TimeSettings
        {
            [InspectorName("Scene Ambient")] public Color ambientColor;
            [InspectorName("Sun Color")] public Color sunColor;
            [InspectorName("Camera Background")] public Color backgroundColor;
            [InspectorName("Sun Intensity")] public float sunIntensity;
            [InspectorName("Shadow Strength"), Range(0f, 1f)] public float shadowStrength;

            [Header("Fog Settings")]
            [InspectorName("Fog Color")] public Color fogColor;
            [InspectorName("Fog Density")] public float fogDensity;

            [Header("Water Settings")]
            [InspectorName("Water Color")] public Color waterColor;
        }

        [Header("Daybreak Settings")]
        public TimeSettings daybreak;

        [Header("Midday Settings")]
        public TimeSettings midday;

        [Header("Sunset Settings")]
        public TimeSettings sunset;

        [Header("Night Settings")]
        public TimeSettings night;

        private static readonly System.Text.StringBuilder _timeStringBuilder = new System.Text.StringBuilder();
        private float _lastTimeUpdated = -1f;

        void Start()
        {
            _timeOfDay = startTimeOfDay;
            UpdateTimeUI();
            UpdateLighting();
        }

        void Update()
        {
            if (_isTimeRunning)
            {
                UpdateTime();
                UpdateLighting();
            }
        }

        private void UpdateTime()
        {
            float internalCycleSpeed = 24f / (minutesPerDay * 60f);
            _timeOfDay += internalCycleSpeed * Time.deltaTime;
            if (_timeOfDay >= 24f) _timeOfDay = 0f;

            int currentMinute = Mathf.FloorToInt((_timeOfDay - Mathf.FloorToInt(_timeOfDay)) * 60);
            if (currentMinute != _lastTimeUpdated)
            {
                _lastTimeUpdated = currentMinute;
                UpdateTimeUI();
            }
        }

        private void UpdateTimeUI()
        {
            if (timeText == null) return;

            int hours = Mathf.FloorToInt(_timeOfDay);
            int minutes = Mathf.FloorToInt((_timeOfDay - hours) * 60);

            _timeStringBuilder.Clear();
            _timeStringBuilder.Append(hours.ToString("00"));
            _timeStringBuilder.Append(":");
            _timeStringBuilder.Append(minutes.ToString("00"));

            timeText.text = _timeStringBuilder.ToString();
        }

        private void UpdateLighting()
        {
            if (sceneCamera == null || sunLight == null) return;

            float timePercent = _timeOfDay / 24f;
            float xRotation = (timePercent * 360f) - 90f;

            if (rotationPivot != null)
                rotationPivot.localRotation = Quaternion.Euler(new Vector3(xRotation, rotationOffsetY, 0));

            if (rotationPivotMoon != null)
            {
                
                float t = Mathf.InverseLerp(moonOffsetTransitionHours.x, moonOffsetTransitionHours.y, _timeOfDay);
                float moonOffset = Mathf.Lerp(moonOffsetMorning, moonOffsetEvening, t);

                rotationPivotMoon.localRotation = Quaternion.Euler(new Vector3(xRotation + moonOffset, rotationOffsetY, 0));
            }

            TimeSettings from, to;
            float blend;

            if (_timeOfDay < 6f) { from = night; to = daybreak; blend = _timeOfDay / 6f; }
            else if (_timeOfDay < 12f) { from = daybreak; to = midday; blend = (_timeOfDay - 6f) / 6f; }
            else if (_timeOfDay < 18f) { from = midday; to = sunset; blend = (_timeOfDay - 12f) / 6f; }
            else { from = sunset; to = night; blend = (_timeOfDay - 18f) / 6f; }

            RenderSettings.ambientLight = Color.Lerp(from.ambientColor, to.ambientColor, blend);
            sunLight.color = Color.Lerp(from.sunColor, to.sunColor, blend);
            sunLight.intensity = Mathf.Lerp(from.sunIntensity, to.sunIntensity, blend);
            sunLight.shadowStrength = Mathf.Lerp(from.shadowStrength, to.shadowStrength, blend);
            sceneCamera.backgroundColor = Color.Lerp(from.backgroundColor, to.backgroundColor, blend);

            if (enableFogControl)
            {
                RenderSettings.fogColor = Color.Lerp(from.fogColor, to.fogColor, blend);
                RenderSettings.fogDensity = Mathf.Lerp(from.fogDensity, to.fogDensity, blend);
            }

            if (moonLight != null)
            {
                float moonIntensity = 0f;

                if (_timeOfDay >= 17f && _timeOfDay < 18f)
                    moonIntensity = 1f + Mathf.InverseLerp(17f, 18f, _timeOfDay);
                else if (_timeOfDay >= 18f || _timeOfDay < 5.5f)
                    moonIntensity = 2.5f;
                else if (_timeOfDay >= 5.5f && _timeOfDay < 6.5f)
                    moonIntensity = 2f - Mathf.InverseLerp(5.5f, 6.5f, _timeOfDay);

                moonLight.intensity = moonIntensity * 0.3f;
                moonLight.color = new Color(0.7f, 0.8f, 1f);
                moonLight.enabled = moonIntensity > 0.01f;
            }

            // URP PhysicallyBasedSky adjustments
            if (RenderSettings.skybox != null && RenderSettings.skybox.shader != null &&
                RenderSettings.skybox.shader.name.Contains("PhysicallyBasedSky"))
            {
                float nightFactor = 0f;
                if (_timeOfDay < 6f) nightFactor = Mathf.SmoothStep(1f, 0f, _timeOfDay / 6f);
                else if (_timeOfDay > 18f) nightFactor = Mathf.SmoothStep(0f, 1f, (_timeOfDay - 18f) / 6f);

                // Adjust exposure
                if (RenderSettings.skybox.HasProperty("_Exposure"))
                {
                    float exposure = Mathf.Lerp(1f, 3f, nightFactor);
                    RenderSettings.skybox.SetFloat("_Exposure", exposure);
                }

                // Tint sky toward night blue
                if (RenderSettings.skybox.HasProperty("_SkyTint"))
                {
                    Color dayTint = Color.white;
                    Color nightTint = new Color(0.08f, 0.12f, 0.25f);
                    RenderSettings.skybox.SetColor("_SkyTint", Color.Lerp(dayTint, nightTint, nightFactor));
                }

                // Adjust ground color
                if (RenderSettings.skybox.HasProperty("_GroundColor"))
                {
                    Color dayGround = new Color(0.6f, 0.5f, 0.45f);
                    Color nightGround = new Color(0.05f, 0.07f, 0.1f);
                    RenderSettings.skybox.SetColor("_GroundColor", Color.Lerp(dayGround, nightGround, nightFactor));
                }

                // Blend fog color slightly toward sky tint
                if (enableFogControl)
                {
                    Color fogBlend = Color.Lerp(RenderSettings.fogColor, new Color(0.05f, 0.08f, 0.12f), nightFactor);
                    RenderSettings.fogColor = fogBlend;
                }

                DynamicGI.UpdateEnvironment();
            }

            // --- Water color ---
            if (waterRenderer != null && waterRenderer.sharedMaterial.HasProperty(waterColorProperty))
            {
                if (_waterPropertyBlock == null)
                    _waterPropertyBlock = new MaterialPropertyBlock();

                waterRenderer.GetPropertyBlock(_waterPropertyBlock);

                Color waterColor = Color.Lerp(from.waterColor, to.waterColor, blend);
                _waterPropertyBlock.SetColor(waterColorProperty, waterColor);
                waterRenderer.SetPropertyBlock(_waterPropertyBlock);
            }

            //Set up nighttime moonlight
            if (RenderSettings.sun != null && moonLight != null)
            {
                if (_timeOfDay >= 17.83f || _timeOfDay < 6.1f)
                {
                    // Nighttime use moon as sun source
                    moonLight.enabled = true;
                    RenderSettings.sun = moonLight;
                    sunLight.enabled = false;
                }
                else
                {
                    // Daytime use actual sun
                    sunLight.enabled = true;
                    RenderSettings.sun = sunLight;
                    moonLight.enabled = false;
                }
            }
        }

        public void StopTime() => _isTimeRunning = false;
        public void StartTime() => _isTimeRunning = true;

        public void ResetTimeSmoothly(float targetTime)
        {
            if (_timeResetCoroutine != null)
                StopCoroutine(_timeResetCoroutine);

            _timeResetCoroutine = StartCoroutine(SmoothTimeReset(targetTime));
        }

        private IEnumerator SmoothTimeReset(float targetTime)
        {
            float originalTime = _timeOfDay;
            float elapsedTime = 0f;

            if (forwardsOnly && originalTime > targetTime)
                targetTime += 24f;

            bool crossesMidnight = (originalTime > targetTime) && (Mathf.Abs(originalTime - targetTime) > 12f);
            float adjustedTargetTime = crossesMidnight ? targetTime + 24f : targetTime;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * resetSpeed;
                _timeOfDay = Mathf.Lerp(originalTime, adjustedTargetTime, elapsedTime) % 24f;
                UpdateLighting();
                yield return null;
            }

            _timeOfDay = targetTime % 24f;
            UpdateLighting();
            UpdateTimeUI();
        }

        public void SetToDaybreak() => SetTimeInstantly(6f);
        public void SetToMidday() => SetTimeInstantly(12f);
        public void SetToSunset() => SetTimeInstantly(18f);
        public void SetToNight() => SetTimeInstantly(0f);

        public void SetTimeInstantly(float targetTime)
        {
            _timeOfDay = targetTime % 24f;
            UpdateLighting();
            UpdateTimeUI();
        }
    }
}
