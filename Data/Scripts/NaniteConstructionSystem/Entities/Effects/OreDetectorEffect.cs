using NaniteConstructionSystem.Extensions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace NaniteConstructionSystem.Entities.Effects
{
    class OreDetectorEffect
    {
        const string EMISSIVE_CORE = "Core_Emissive";
        const string EMISSIVE_COOLER = "Cooler_Emissive";
        const string EMISSIVE_PILLAR = "Pillar_Emissive";
        const string EMISSIVE_COIL_PREFIX = "Coil_Emissive";
        const int MAX_COILS = 8;
        enum EffectState
        {
            Unkown,
            Active,
            Inactive,
            Scanning,
            ScanComplete
        }

        private MyCubeBlock m_block;

        public OreDetectorEffect(MyCubeBlock block)
        {
            m_block = block;
        }

        private int m_updateCount;
        public int UpdateCount
        {
            get { return m_updateCount; }
        }

        const float COOLER_MAX_COUNT = 4000f;
        private short m_coolerStatusCount;
        private EffectState m_state = EffectState.Unkown;

        public void ActiveUpdate()
        {
            DrawCooler(false);

            if (m_state != EffectState.Active)
            {
                m_block.SetEmissiveParts(EMISSIVE_PILLAR, Color.Green, 1f);
                m_block.SetEmissiveParts(EMISSIVE_CORE, Color.Black, 1f);
                m_updateCount = 0;
                DrawCoil(false);
                m_state = EffectState.Active;
            }
        }

        public void InactiveUpdate()
        {
            DrawCooler(false);

            if (m_state != EffectState.Inactive)
            {
                m_block.SetEmissiveParts(EMISSIVE_PILLAR, Color.Red, 1f);
                m_block.SetEmissiveParts(EMISSIVE_CORE, Color.Black, 1f);
                m_updateCount = 0;
                DrawCoil(false);
                m_state = EffectState.Inactive;
            }
        }

        public void ScanningUpdate()
        {
            if (m_state != EffectState.Scanning)
                m_state = EffectState.Scanning;

            m_block.SetEmissiveParts(EMISSIVE_CORE, Color.Blue, MathExtensions.TrianglePulse(m_updateCount, 0.8f, 150));
            DrawCooler(true);
            DrawCoil(true);
            m_updateCount++;
        }

        public void ScanCompleteUpdate()
        {
            DrawCooler(false);

            if (m_state != EffectState.ScanComplete)
            {
                m_block.SetEmissiveParts(EMISSIVE_CORE, Color.Aquamarine, 1f);
                m_updateCount = 0;
                DrawCoil(false);
                m_state = EffectState.ScanComplete;
            }
        }

        private void DrawCooler(bool inUse)
        {
            // should take 3 sec to "heat" up
            if (inUse && m_coolerStatusCount < COOLER_MAX_COUNT)
                m_coolerStatusCount++;
            else if (!inUse && m_coolerStatusCount > 0)
                m_coolerStatusCount--;
            else if (m_state == EffectState.Unkown)
            {
                m_block.SetEmissiveParts(EMISSIVE_COOLER, Color.Black, 1f);
                return;
            }
            else
                return;

            Color heatColor = Color.DarkRed;
            float emissivity = 0.2f;
            if (m_coolerStatusCount > 0 && m_coolerStatusCount != COOLER_MAX_COUNT)
            {
                float rFactor = (float)heatColor.R / COOLER_MAX_COUNT;
                float gFactor = (float)heatColor.G / COOLER_MAX_COUNT;
                float bFactor = (float)heatColor.B / COOLER_MAX_COUNT;
                heatColor.R = (byte)(rFactor * (float)m_coolerStatusCount);
                heatColor.G = (byte)(gFactor * (float)m_coolerStatusCount);
                heatColor.B = (byte)(bFactor * (float)m_coolerStatusCount);
                float emissivityFactor = emissivity / COOLER_MAX_COUNT;
                emissivity = emissivityFactor * m_coolerStatusCount;
            }
            else if (m_coolerStatusCount == 0)
            {
                heatColor.R = 0;
                heatColor.G = 0;
                heatColor.B = 0;
            }

            m_block.SetEmissiveParts(EMISSIVE_COOLER, heatColor, emissivity);
        }

        private void DrawCoil(bool active)
        {
            if (!active)
            {
                for (int i = 0; i < MAX_COILS; i++)
                    m_block.SetEmissiveParts($"{EMISSIVE_COIL_PREFIX}{i}", Color.Black, 1f);
                return;
            }

            int updateCount = m_updateCount >> 1;

            int iteration = (updateCount + 1) % MAX_COILS;
            int frontIteration = (updateCount + 2) % MAX_COILS;
            int rearIteration = updateCount % MAX_COILS;
            for (int i = 0; i < MAX_COILS; i++)
            {
                if (i == frontIteration || i == rearIteration)
                    m_block.SetEmissiveParts($"{EMISSIVE_COIL_PREFIX}{i}", Color.DarkBlue, 0.125f);
                else if (i == iteration)
                    m_block.SetEmissiveParts($"{EMISSIVE_COIL_PREFIX}{i}", Color.Blue, 0.5f);
                else
                    m_block.SetEmissiveParts($"{EMISSIVE_COIL_PREFIX}{i}", Color.Black, 1f);
            }
        }
    }
}