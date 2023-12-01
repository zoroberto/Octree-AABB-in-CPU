using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    /////////////////////////////
    public struct ParticleList
    {
        public List<Particle> particle;
    }

    public class Particle
    {
        private Vector3 pos;
        private Vector3 velo;

        public Vector3 POS
        {
            get { return pos; }
            set { pos = value; }
        }

        public Vector3 VELO
        {
            get { return velo; }
            set { velo = value; }
        }

        public Particle()
        {
            // Initialize pos and velo as needed
        }

        public void UpdatePosition(float dt, Vector3 gravity)
        {
            velo += dt * gravity;
            pos += dt * velo;
        }

        public void UpdateReverseVelocity(float dt)
        {
            // + offset of objects, velo.y = target + 15;

            velo *= -1f;
            pos += dt * velo;
        }

    }
}
