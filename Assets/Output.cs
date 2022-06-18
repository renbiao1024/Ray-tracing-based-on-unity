using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class Output : MonoBehaviour
{
    string IMG_PATH;
    int WIDTH;
    int HEIGHT;

    //视锥体的左下角、长宽和起始扫射点设定
    Vector3 lowLeftCorner;
    Vector3 horizontal;
    Vector3 vertical;

    //光源位置
    Vector3 original;

    //采样数，采样权重
    int SAMPLE;
    float SAMPLE_WEIGHT;

    //散射次数
    int MAX_SCATTER_TIME;

    public Output()
    {
        IMG_PATH = "D:\\UnityProject\\RayTrace\\Assets\\pictures\\test8.PNG";
        WIDTH = 200;
        HEIGHT = 100;
        lowLeftCorner = new Vector3(-2, -1, -1);
        horizontal = new Vector3(4, 0, 0);
        vertical = new Vector3(0, 2, 0);
        original = new Vector3(0, 0, 0);
        SAMPLE = 100;
        SAMPLE_WEIGHT = 1f / SAMPLE;
        MAX_SCATTER_TIME = 5;
    }
    // Start is called before the first frame update
    void Start()
    {

        //CreatePng(WIDTH, HEIGHT, CreateColorForTextPNG(WIDTH, HEIGHT));

        //CreatePng(WIDTH, HEIGHT, CreateColorForTestRay(WIDTH, HEIGHT));

        //CreatePng(WIDTH, HEIGHT, CreateColorForTestSphere(WIDTH, HEIGHT));

        //CreatePng(WIDTH, HEIGHT, CreateColorForTestNormal(WIDTH, HEIGHT));

        //CreatePng(WIDTH, HEIGHT, CreateColorForTestHitRecord(WIDTH, HEIGHT));

        //CreatePng(WIDTH, HEIGHT, CreateColorForTestAntialiasing(WIDTH, HEIGHT));

        //CreatePng(WIDTH, HEIGHT, CreateColorForTesstDiffusing(WIDTH, HEIGHT));

        CreatePng(WIDTH, HEIGHT, CreateColorForTestMetal(WIDTH, HEIGHT));


    }

    //输出PNG
    void CreatePng(int width, int height, Color[] colors)
    {
        if (width * height != colors.Length)
        {
            EditorUtility.DisplayDialog("Error", "长宽和数组无法对应", "ok");
            return;
        }
        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        tex.SetPixels(colors);
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        FileStream fs = new FileStream(IMG_PATH, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(bytes);
        fs.Close();
        bw.Close();
    }

    //PNG测试
    Color[] CreateColorForTextPNG(int width, int height)
    {
        int l = width * height;
        Color[] colors = new Color[l];
        for (int j = height - 1; j >= 0; --j)
        {
            for (int i = 0; i < width; ++i)
            {
                colors[i + j * width] = new Color(i / (float)width, j / (float)height, 0.2f);
            }
        }
        return colors;
    }

    //光线测试
    Color GetColorForTestRay(Ray ray)
    {
        float t = 0.5f * (ray.normalDirection.y + 1f);
        return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
    }
    Color[] CreateColorForTestRay(int width, int height)
    {
        int l = width * height;
        Color[] colors = new Color[l];
        for (int j = height - 1; j >= 0; j--)
            for (int i = 0; i < width; i++)
            {
                Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                colors[i + j * width] = GetColorForTestRay(r);
            }
        return colors;
    }

    //球体测试
    bool isHitSphereForTestSphere(Vector3 center, float radius, Ray ray)
    {
        //光线和球相切时: dot(o + td - C, o + td - C) = R*R
        //展开：t*t*dot(d,d)+2*t*dot(d,o-C)+dot(o-C,o-C)-R*R = 0
        var oc = ray.original - center;
        float a = Vector3.Dot(ray.direction, ray.direction);
        float b = 2f * Vector3.Dot(oc, ray.direction);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        //实际上是判断这个方程有没有根，如果有2个根就是击中
        float discriminant = b * b - 4 * a * c;
        return (discriminant >= 0);
    }
    Color GetColorForTestSphere(Ray ray)
    {
        if (isHitSphereForTestSphere(new Vector3(0, 0, -1), 0.5f, ray))
        {
            return new Color(1, 0, 0);//red
        }
        return GetColorForTestRay(ray);
    }
    Color[] CreateColorForTestSphere(int width, int height)
    {
        int l = width * height;
        Color[] colors = new Color[l];
        for (int j = height - 1; j >= 0; --j)
        {
            for (int i = 0; i < width; ++i)
            {
                Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                colors[i + j * width] = GetColorForTestSphere(r);
            }
        }
        return colors;
    }

    //法线测试
    float HitSphereForTestNormal(Vector3 center, float radius, Ray ray)
    {
        var oc = ray.original - center;
        float a = Vector3.Dot(ray.direction, ray.direction);
        float b = 2f * Vector3.Dot(oc, ray.direction);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0) return -1;
        return (-b - Mathf.Sqrt(discriminant)) / (2f * a);
    }
    Color GetColorForTestNormal(Ray ray)
    {
        Vector3 c = new Vector3(0, 0, -1);
        float t = HitSphereForTestNormal(c, 0.5f, ray);
        if (t > 0)
        {
            Vector3 normal = Vector3.Normalize(ray.GetPoint(t) - c);
            return 0.5f * new Color(normal.x + 1, normal.y + 1, normal.z + 1, 2f);
        }
        return GetColorForTestRay(ray);
    }
    Color[] CreateColorForTestNormal(int width, int height)
    {
        //视锥体的左下角、长宽和起始扫射点设定
        Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
        Vector3 horizontal = new Vector3(4, 0, 0);
        Vector3 vertical = new Vector3(0, 2, 0);
        Vector3 original = new Vector3(0, 0, 0);
        int l = width * height;
        Color[] colors = new Color[l];
        for (int j = height - 1; j >= 0; j--)
            for (int i = 0; i < width; i++)
            {
                Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                colors[i + j * width] = GetColorForTestNormal(r);
            }
        return colors;
    }

    //球体hit测试
    Color GetColorForTestHitRecord(Ray ray, HitableList hitableList)
    {
        HitRecord record = new HitRecord();
        if (hitableList.Hit(ray, 0f, float.MaxValue, ref record))
        {
            return 0.5f * new Color(record.normal.x + 1, record.normal.y + 1, record.normal.z + 1, 2f);
        }
        return GetColorForTestRay(ray);
    }
    Color[] CreateColorForTestHitRecord(int width, int height)
    {
        int l = width * height;
        HitableList hitableList = new HitableList();
        //添加两个球体，一个当地面，一个在中间展示
        hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f));
        hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f));

        Color[] colors = new Color[l];
        for (int j = height - 1; j >= 0; j--)
        {
            for (int i = 0; i < width; i++)
            {
                Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                colors[i + j * width] = GetColorForTestHitRecord(r, hitableList);
            }
        }
        return colors;
    }

    //抗锯齿测试，每像素n条射线的蒙特卡罗方法(概率统计)
    Color GetColorForTestAntialiasing(Ray ray, HitableList hitableList)
    {
        HitRecord record = new HitRecord();
        if (hitableList.Hit(ray, 0f, float.MaxValue, ref record))
        {
            return 0.5f * new Color(record.normal.x + 1, record.normal.y + 1, record.normal.z + 1, 2f);
        }
        return GetColorForTestRay(ray);
    }
    Color[] CreateColorForTestAntialiasing(int width, int height)
    {
        int l = width * height;
        HitableList hitableList = new HitableList();
        hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f));
        hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f));
        Color[] colors = new Color[l];

        for (int j = height - 1; j >= 0; j--)
        {
            for (int i = 0; i < width; i++)
            {
                Color color = new Color(0, 0, 0);
                for (int s = 0; s < SAMPLE; s++)
                {
                    Ray r = new Ray(original, lowLeftCorner + horizontal * (i + Random.Range(0f, 1f)) / (float)width + vertical * (j + Random.Range(0f, 1f)) / (float)height);
                    color += GetColorForTestAntialiasing(r, hitableList);
                }
                color *= SAMPLE_WEIGHT;
                color.a = 1f;
                colors[i + j * width] = color;
            }
        }
        return colors;
    }

    //漫散射测试
    Vector3 GetRandomPointInUnitSphereForTestDiffusing()
    {
        Vector3 p = 2f * new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)) - Vector3.one;//球心
        p = p.normalized * Random.Range(0f, 1f);//球心到球内一点的随机向量
        return p;
    }
    Color GetColorForTestDiffusing(Ray ray, HitableList hitableList)
    {
        HitRecord record = new HitRecord();
        if (hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
        {
            Vector3 target = record.p + record.normal + GetRandomPointInUnitSphereForTestDiffusing(); //交点->法线 + 单位圆的一点 

            return 0.5f * GetColorForTestDiffusing(new Ray(record.p, target - record.p), hitableList);
        }
        return GetColorForTestRay(ray);
    }
    Color[] CreateColorForTesstDiffusing(int width, int height)
    {
        int l = width * height;
        HitableList hitableList = new HitableList();
        hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f));
        hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f));
        Color[] colors = new Color[l];

        for (int j = height - 1; j >= 0; j--)
            for (int i = 0; i < width; i++)
            {
                Color color = new Color(0, 0, 0);
                for (int s = 0; s < SAMPLE; s++)
                {
                    Ray r = new Ray(original, lowLeftCorner + horizontal * (i + Random.Range(0f, 1f)) / (float)width + vertical * (j + Random.Range(0f, 1f)) / (float)height);
                    color += GetColorForTestDiffusing(r, hitableList);
                }
                color *= SAMPLE_WEIGHT;
                //为了使球体看起来更亮，改变gamma值
                //color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                color.a = 1f;
                colors[i + j * width] = color;
            }
        return colors;
    }

    //镜面反射测试
    Color GetColorForTestMetal(Ray ray, HitableList hitableList, int depth)
    {
        HitRecord record = new HitRecord();
        if (hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
        {
            Ray r = new Ray(Vector3.zero, Vector3.zero);
            Color attenuation = Color.black;

            if (depth < MAX_SCATTER_TIME && record.material.scatter(ray, record, ref attenuation, ref r))
            {
                Color c = GetColorForTestMetal(r, hitableList, depth + 1);
                return new Color(c.r * attenuation.r, c.g * attenuation.g, c.b * attenuation.b);
            }
            else
            {
                //假设已经反射了太多次，或者压根就没有发生反射，那么就认为黑了
                return Color.black;
            }
        }
        return GetColorForTestRay(ray);
    }
    Color[] CreateColorForTestMetal(int width, int height)
    {
        int l = width * height;
        HitableList hitableList = new HitableList();
        hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Color(0.8f, 0.3f, 0.3f))));
        hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f, new Lambertian(new Color(0.8f, 0.8f, 0.0f))));
        hitableList.list.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.6f, 0.2f))));
        hitableList.list.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.8f, 0.8f))));
        Color[] colors = new Color[l];

        for (int j = height - 1; j >= 0; j--)
            for (int i = 0; i < width; i++)
            {
                Color color = new Color(0, 0, 0);
                for (int s = 0; s < SAMPLE; s++)
                {
                    Ray r = new Ray(original, lowLeftCorner + horizontal * (i + Random.Range(0f, 1f)) / (float)width + vertical * (j + Random.Range(0f, 1f)) / (float)height);
                    color += GetColorForTestMetal(r, hitableList, 0);
                }
                color *= SAMPLE_WEIGHT;
                color.a = 1f;
                colors[i + j * width] = color;
            }
        return colors;
    }
}