using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace newtracer
{
    public class Sector
    {
        private Vector3 tl { get; set; }
        private Vector3 tr { get; set; }
        private Vector3 bl { get; set; }
        private Vector3 br { get; set; }
        private int minHit { get; set; }
        private int maxHit { get; set; }
        private Vector3 color;
        public Vector3 Color
        {
            get { return color; }
            set { color = value; }
        }
        private int hitCounter { get; set; }
        private Vector3 center { get; set; }
        private float sqr { get; set; } //square of the sector (via 2 triangles)
        public float getSqr { get { return sqr; } }
        public int HitCounter
        {
            get { return hitCounter; }
        }
        public void IncHitCounter()
        {
            hitCounter += 1;
        }
        public Sector()
        {
            tl = new Vector3();
            tr = new Vector3();
            bl = new Vector3();
            br = new Vector3();

            color = new Vector3();
            center = new Vector3(0, 0, -10);
            hitCounter = 0;
        }
        //public Sector(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4) : base()
        //{
        //    tl = p1;
        //    tr = p2;
        //    bl = p3;
        //    br = p4;
        //    calcSqr();
        //    //center = new Vector3((p1.X + p4.X) / 2, (p1.Y + p4.Y) / 2, (p1.Z + p4.Z) / 2);
        //}

        public Sector(float radius, Vector3 pos, float theta, float phi, float phi_stp, float theta_stp) : base()    //create sector for sphercal surface
        {
            //float phi_stp, theta_stp;
            center = pos;
            Vector3 p1, p2, p3, p4; // 
            //float cir_len = (float)Math.PI * 2.0f * radius;
            //phi_stp = cir_len / (float)div_vert;    //phi - vertical layout
            //theta_stp = 2 * radius / (float)div_hor;

            p1.X = center.X + radius * (float)(Math.Sin(Math.Acos(theta)) * Math.Cos(phi));
            p1.Y = center.Y + radius * (float)theta;
            p1.Z = center.Z + radius * (float)(Math.Sin(Math.Acos(theta)) * Math.Sin(phi));

            p2.X = center.X + radius * (float)(Math.Sin(Math.Acos(theta)) * Math.Cos(phi + phi_stp));
            p2.Y = center.Y + radius * (float)theta;
            p2.Z = center.Z + radius * (float)(Math.Sin(Math.Acos(theta)) * Math.Sin(phi + phi_stp));

            p3.X = center.X + radius * (float)(Math.Sin(Math.Acos(theta) + theta_stp) * Math.Cos(phi));
            p3.Y = center.Y + radius * (float)theta;
            p3.Z = center.Z + radius * (float)(Math.Sin(Math.Acos(theta) + theta_stp) * Math.Sin(phi));

            p4.X = center.X + radius * (float)(Math.Sin(Math.Acos(theta) + theta_stp) * Math.Cos(phi + phi_stp));
            p4.Y = center.Y + radius * (float)theta;
            p4.Z = center.Z + radius * (float)(Math.Sin(Math.Acos(theta) + theta_stp) * Math.Sin(phi + phi_stp));

            tl = new Vector3(p1.X, p1.Y, p1.Z);
            tr = new Vector3(p2.X, p2.Y, p2.Z);
            bl = new Vector3(p3.X, p3.Y, p3.Z);
            br = new Vector3(p4.X, p4.Y, p4.Z);
            //Sector res = new Sector(p1, p2, p3, p4);
            //return res;
        }

        public bool intersectionTest(Vector3 p)
        {
            var N_1 = Vector3.Normalize(Vector3.Cross(tl - bl, tl - tr));   //1st triangle normal
            var N_2 = Vector3.Normalize(Vector3.Cross(br - bl, br - tr));   //2nd triangle normal

            var nom = Vector3.Dot(tl - center, N_1);
            var denom = Vector3.Dot(p, N_1);
            var u = nom / denom;

            if (u >= 0) //1st triangle check
                return true;
            else
            {
                nom = Vector3.Dot(br - center, N_2);    //2nd triangle check
                denom = Vector3.Dot(p, N_2);
                u = nom / denom;
                if (u >= 0)
                    return true;
            }

            return false;   //no intersection found
        }

        public bool baricTest(Vector3 p)
        {
            //sides of triangles
            float s1, s2, s3, s4, s5;
            s1 = (float)Math.Sqrt(Math.Pow(tl.X - bl.X, 2) + Math.Pow(tl.Y - bl.Y, 2) + Math.Pow(tl.Z - bl.Z, 2));    //TL-BL
            s2 = (float)Math.Sqrt(Math.Pow(tl.X - tr.X, 2) + Math.Pow(tl.Y - tr.Y, 2) + Math.Pow(tl.Z - tr.Z, 2));  //TL-TR
            s3 = (float)Math.Sqrt(Math.Pow(tr.X - bl.X, 2) + Math.Pow(tr.Y - bl.Y, 2) + Math.Pow(tr.Z - bl.Z, 2));  //TR-BL
            s4 = (float)Math.Sqrt(Math.Pow(bl.X - br.X, 2) + Math.Pow(bl.Y - br.Y, 2) + Math.Pow(bl.Z - br.Z, 2));  //BL-BR
            s5 = (float)Math.Sqrt(Math.Pow(tr.X - br.X, 2) + Math.Pow(tr.Y - br.Y, 2) + Math.Pow(tr.Z - br.Z, 2));  //TR-BR

            float sp1 = 0, sp2 = 0; //semi-perimeters
            sp1 = (s1 + s2 + s3) / 2;
            sp2 = (s3 + s4 + s5) / 2;

            float sq1 = 0, sq2 = 0; //Left and Right triangles squares via Heron
            sq1 = (float)Math.Sqrt(sp1 * (sp1 - s1) * (sp1 - s2) * (sp1 - s3));
            sq2 = (float)Math.Sqrt(sp2 * (sp2 - s3) * (sp2 - s4) * (sp2 - s5));

            //temporals
            float ts1, ts2, ts3;    //temp sides for inter triangles
            ts1 = (float)Math.Sqrt(Math.Pow(tl.X - p.X, 2) + Math.Pow(tl.Y - p.Y, 2) + Math.Pow(tl.Z - p.Z, 2));
            ts2 = (float)Math.Sqrt(Math.Pow(bl.X - p.X, 2) + Math.Pow(bl.Y - p.Y, 2) + Math.Pow(bl.Z - p.Z, 2));
            ts3 = (float)Math.Sqrt(Math.Pow(tr.X - p.X, 2) + Math.Pow(tr.Y - p.Y, 2) + Math.Pow(tr.Z - p.Z, 2));

            float tsp = 0;  //temp semiperimeter
            float tsq1, tsq2, tsq3; //temp squares

            tsp = (ts1 + ts3 + s1) / 2;
            tsq1 = tsp * (tsp - s1) * (tsp - ts1) * (tsp - ts3);
            tsq1 = tsq1 > 0 ? (float)Math.Sqrt(tsq1) : 0;
            //
            tsp = (ts1 + ts2 + s2) / 2;
            tsq2 = tsp * (tsp - s2) * (tsp - ts1) * (tsp - ts2);
            tsq2 = tsq2 > 0 ? (float)Math.Sqrt(tsq2) : 0;
            //
            tsp = (ts2 + ts3 + s3) / 2;
            tsq3 = tsp * (tsp - s3) * (tsp - ts2) * (tsp - ts3);
            tsq3 = tsq3 > 0 ? (float)Math.Sqrt(tsq3) : 0;

            float tsq;
            tsq = tsq1 + tsq2 + tsq3;

            if (Math.Abs(tsq - sq1) <= 0.01) //check 1st triangle using barycentric rule
            {
                hitCounter += 1;
                return true;
            }
            else
            {
                ts1 = (float)Math.Sqrt(Math.Pow(tr.X - p.X, 2) + Math.Pow(tr.Y - p.Y, 2) + Math.Pow(tr.Z - p.Z, 2));
                ts2 = (float)Math.Sqrt(Math.Pow(br.X - p.X, 2) + Math.Pow(br.Y - p.Y, 2) + Math.Pow(br.Z - p.Z, 2));
                ts3 = (float)Math.Sqrt(Math.Pow(bl.X - p.X, 2) + Math.Pow(bl.Y - p.Y, 2) + Math.Pow(bl.Z - p.Z, 2));

                tsp = (ts1 + ts3 + s3) / 2;
                tsq1 = tsp * (tsp - s3) * (tsp - ts1) * (tsp - ts3);
                tsq1 = tsq1 > 0 ? (float)Math.Sqrt(tsq1) : 0;
                //
                tsp = (ts1 + ts2 + s5) / 2;
                tsq2 = tsp * (tsp - s5) * (tsp - ts1) * (tsp - ts2);
                tsq2 = tsq2 > 0 ? (float)Math.Sqrt(tsq2) : 0;
                //
                tsp = (ts2 + ts3 + s4) / 2;
                tsq3 = tsp * (tsp - s4) * (tsp - ts2) * (tsp - ts3);
                tsq3 = tsq3 > 0 ? (float)Math.Sqrt(tsq3) : 0;

                tsq = tsq1 + tsq2 + tsq3;

                if (Math.Abs(tsq - sq2) <= 0.01)
                {
                    hitCounter += 1;
                    return true;
                }
            }
            return false;
        }

        public void enlighten(float val)
        {
            float R, G, B;
            R = color.X + val;
            G = color.Y + val;
            B = color.Z + val;

            color = new Vector3(R, G, B);
        }
        private void calcSqr()
        {
            float s1, s2, s3, s4, s5;
            s1 = (tl - bl).Length();    //TL-BL
            s2 = (tl - tr).Length();  //TL-TR
            s3 = (tr - bl).Length();  //TR-BL
            s4 = (bl - br).Length();  //BL-BR
            s5 = (tr - br).Length();  //TR-BR

            float sp1 = 0, sp2 = 0; //semi-perimeters
            sp1 = (s1 + s2 + s3) / 2;
            sp2 = (s3 + s4 + s5) / 2;

            float sq1 = 0, sq2 = 0; //Left and Right triangles squares via Heron
            sq1 = (float)Math.Sqrt(sp1 * (sp1 - s1) * (sp1 - s2) * (sp1 - s3));
            sq2 = (float)Math.Sqrt(sp2 * (sp2 - s3) * (sp2 - s4) * (sp2 - s5));

            sqr = sq1 + sq2;
        }
    }

}
