using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;


namespace newtracer
{
    public class obj
    {
        //protected Vector3 center { get {return } set { center = new Vector3(); } }
        protected Vector3 center { get; set; }
    }

    public class geometry : obj
    {
        protected int div_hor { get; set; } //horizontal surface division
        protected int div_vert { get; set; }    //vertical surface division
        protected Material material { get; set; }
        protected List<Sector> wireframe;

        public geometry(Vector3 c, Material m) : base()
        {
            wireframe = new List<Sector>();
            div_hor = 10;   //for example. Should be changed in constructors
            div_vert = 10;
            center = c;
            material = m;
        }
    }

    public class Light : obj
    {
        private double intensity;
        //private Vector3 position;
        //protected Vector3 center;

        public Light(Vector3 p, float i) : base()
        {
            center = p;
            intensity = i;
        }

        public double Intensity
        {
            get
            {
                return intensity;
            }
        }
        public Vector3 Center
        {
            get
            {
                return center;
            }
        }

    }

    public class Material
    {
        //private Vector2 albedo; //diffuse and specular
        private Vector3 albedo;
        //X-diffuse component (kd)
        //Y-specular component (ks)
        //Z-reflective component
        private Vector3 diffuse_color;
        private float specular_exponent;    // value that controls the apparent smoothness of the surface

        public Material()
        {
            //albedo = new Vector2(1, 0);
            albedo = new Vector3(1, 0, 0);
            diffuse_color = new Vector3(1f, 1f, 1f);
            specular_exponent = 0;
        }

        //public Material(Vector2 a, Vector3 color, float spec):base()
        public Material(Vector3 a, Vector3 color, float spec) : base()
        {
            albedo = a;
            diffuse_color = color;
            specular_exponent = spec;
        }

        //public Vector2 Albedo
        public Vector3 Albedo
        {
            get { return albedo; }
        }
        public Vector3 Diffuse_color
        {
            get { return diffuse_color; }
        }
        public double Specular_exponent
        {
            get { return specular_exponent; }
        }
    }

    public class Sphere : geometry
    {
        private float radius { get; set; }

        public Sphere(Vector3 c, float r, Material m) : base(c, m)
        {
            div_hor = 160;
            div_vert = 40;
            radius = r;

            createWireframe();
        }

        private void createWireframe()
        {
            float cir_len = (float)Math.PI * 2.0f * radius;
            float theta_stp = 2 * radius / (float)div_hor;
            float phi_stp = cir_len / (float)div_vert;    //phi - vertical layout
            Sector tmp = new Sector();
            for (float theta = -radius; theta < radius; theta += theta_stp)  //creating wireframe
            {
                for (float phi = 0; phi < 2.0 * Math.PI; phi += phi_stp)
                {
                    tmp = new Sector(radius, center, theta, phi, theta_stp, phi_stp);// ЗАМЕНИТЬ КОНСТРУКТОР НА МЕТОД ИЛИ ВООБЩЕ ВЫНЕСТИ В СТАТИК!
                    wireframe.Add(tmp);
                }
            }
        }

        public bool ray_intersect(Vector3 orig, Vector3 dir, ref double t0) //check if the ray intersects the sphere
        {
            Vector3 L = center - orig;
            //float tca = L * dir;
            double tca = System.Numerics.Vector3.Dot(L, dir);
            //float d2 = L * L - tca * tca;
            double d2 = Vector3.Dot(L, L) - tca * tca;
            if (d2 > radius * radius) return false;
            double thc = Math.Sqrt(radius * radius - d2);
            t0 = tca - thc;
            double t1 = tca + thc;
            if (t0 < 0) t0 = t1;
            if (t0 < 0) return false;
            return true;
        }
    }

    public class Illuminance
    {
        private Vector2 pos;
        private Vector3 RGB;

        public Illuminance()
        {
            pos = new Vector2();
            //pos = p;
            RGB = new Vector3();
            //RGB = color;
        }
        public Illuminance(Vector2 p, Vector3 color) : base()
        {
            pos = p;
            RGB = color;
        }

        public void set(Vector2 p, Vector3 color)
        {
            pos = p;
            RGB = color;
        }
        public Vector2 getPos()
        {
            return pos;
        }
        public Vector3 getRGB()
        {
            return RGB;
        }
    }
    class scene
    {
        List<Sphere> sphereColl;
        List<Light> lightColl;
        List<Illuminance> illuminances;

        double fov;
        int win_w, win_h;
        double dir_x;
        double dir_y;
        double dir_z;

        public double X { get { return dir_x; } }
        public double Y { get { return dir_y; } }
        public double Z { get { return dir_z; } }
        public int Width { get { return win_w; } }
        public int Height { get { return win_h; } }
        public List<Sphere> Spheres { get { return sphereColl; } }
        public List<Light> Lights { get { return lightColl; } }
        public double FOV { get { return fov; } }
        public void rayCasting(int win_w, int win_h)
        {
            Vector3 origin = new Vector3(0, 0, 0);
            Vector3 direction = new Vector3(0, 0, 0);
            Vector3 res_color = new Vector3();
            Vector3 norm_dir = new Vector3();

            for (int j = 0; j < win_h; j++)
            {
                for (int i = 0; i < win_w; i++)
                {
                    dir_x = (i + 0.5) - win_w / 2f;
                    dir_y = -(j + 0.5) + win_h / 2f;
                    dir_z = -win_h / (2f * Math.Tan(fov / 2f));

                    direction.X = (float)dir_x;
                    direction.Y = (float)dir_y;
                    direction.Z = (float)dir_z;

                    norm_dir = Vector3.Normalize(direction);
                    //res_color = interactions.castDepthRay(ref origin, ref norm_dir, sphereColl);
                    res_color = interactions.cast_ray(origin, ref norm_dir, sphereColl, lightColl);

                }
            }
        }

        public List<Sphere> sphereCollGen()
        {
            //Material ivory = new Material(new Vector2(0.6f, 0.3f), new Vector3(0.4f, 0.4f, 0.3f), 50);
            //Material rubber_green = new Material(new Vector2(0.4f, 0.3f), new Vector3(0.1f, 1f, 0.1f), 100);
            //Material green_1 = new Material(new Vector2(0.7f, 0.1f), new Vector3(0.1f, 1f, 0.1f), 50);
            //Material green_2 = new Material(new Vector2(0.7f, 15f), new Vector3(0.1f, 1f, 0.1f), 50);

            Material ivory = new Material(new Vector3(0.6f, 0.3f, 0.0f), new Vector3(0.4f, 0.4f, 0.3f), 50f);
            Material rubber_green = new Material(new Vector3(0.9f, 0.1f, 0f), new Vector3(0.1f, 0.9f, 0.1f), 10f);
            Material red_1 = new Material(new Vector3(0.45f, 0.5f, 0f), new Vector3(1f, 0f, 0f), 50);
            Material red_2 = new Material(new Vector3(0.5f, 0.5f, 0f), new Vector3(1f, 0f, 0f), 500);

            List<Sphere> spheres = new List<Sphere>();
            Sphere temp = new Sphere(new Vector3(0, 0, -5), 1f, ivory);
            spheres.Add(temp);

            ////add more spheres here by modifying and copy-pasting temp
            temp = new Sphere(new Vector3(2, -2, -40), 20f, rubber_green);
            spheres.Add(temp);
            temp = new Sphere(new Vector3(-2, 2, -10), 1f, red_2);
            spheres.Add(temp);
            temp = new Sphere(new Vector3(-4, 4, -12), 1f, red_1);
            spheres.Add(temp);


            return spheres;
        }

        public List<Light> lightCollGen()
        {
            List<Light> lights = new List<Light>();
            //Light temp = new Light();

            //var temp = new Light(new Vector3(5, 10, 20), 10f);
            var temp = new Light(new Vector3(0, 3, 20), 100000f);
            lights.Add(temp);
            //temp = new Light(new Vector3(-20, 40, 60), 1.5f);
            //lights.Add(temp);

            return lights;
        }

        public scene()
        {
            fov = Math.PI / 2f;
            win_w = 512;
            win_h = 512;

            dir_x = 0f;
            dir_y = 0f;
            dir_z = 0f;

            sphereColl = sphereColl = sphereCollGen();
            lightColl = lightCollGen();
        }
    }
}