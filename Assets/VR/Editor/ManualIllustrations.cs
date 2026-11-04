using UnityEngine;
namespace HospitalVR.Editor
{
    /// <summary>Gráficos originales de líneas; cada página comparte el mismo marco local.</summary>
    public static class ManualIllustrations
    {
        public static Transform Make(Transform parent,int page)
        {
            var root=new GameObject("Ilustracion_"+page).transform; root.SetParent(parent,false); root.localPosition=new Vector3(0,.15f,-.045f);
            switch(page)
            {
                case 0:
                    Touch(root,-.105f); Touch(root,.105f);
                    Label(root,"GRIP",new Vector2(0,-.055f)); break;
                case 1:
                    Person(root,-.1f,0); Circle(root,new Vector2(.09f,.04f),.035f);
                    Line(root,new Vector2(.04f,-.02f),new Vector2(.15f,-.02f));
                    Label(root,"OBSERVA · RESPUESTA · RESPIRA",new Vector2(0,-.075f)); break;
                case 2:
                    Box(root,new Vector2(-.09f,.005f),new Vector2(.07f,.12f)); Person(root,.095f,0);
                    Label(root,"UBICACIÓN + SITUACIÓN + DEA",new Vector2(0,-.075f)); break;
                case 3:
                    Circle(root,new Vector2(-.11f,.055f),.025f);
                    Box(root,new Vector2(-.11f,-.015f),new Vector2(.10f,.09f));
                    Circle(root,new Vector2(-.11f,-.005f),.014f);
                    Line(root,new Vector2(.02f,-.045f),new Vector2(.17f,-.045f));
                    Line(root,new Vector2(.08f,.065f),new Vector2(.105f,.04f),new Vector2(.105f,-.04f));
                    Label(root,"MARCADOR AZUL · POSTURA",new Vector2(0,-.075f)); break;
                case 4:
                    Line(root,new Vector2(-.18f,-.04f),new Vector2(-.03f,-.04f));
                    Arrow(root,new Vector2(-.105f,.065f),new Vector2(-.105f,-.025f));
                    Line(root,new Vector2(.03f,-.04f),new Vector2(.18f,-.04f));
                    Arrow(root,new Vector2(.105f,-.025f),new Vector2(.105f,.065f));
                    Label(root,"DESCENSO              RETORNO",new Vector2(0,-.075f)); break;
                case 5:
                    Box(root,new Vector2(-.10f,0),new Vector2(.1f,.12f));
                    Box(root,new Vector2(.095f,0),new Vector2(.10f,.12f));
                    Circle(root,new Vector2(.07f,.025f),.015f); Circle(root,new Vector2(.12f,-.03f),.015f);
                    Label(root,"ENCENDER · PARCHES · APARTARSE",new Vector2(0,-.075f)); break;
            }
            return root;
        }
        static void Touch(Transform r,float x) { Circle(r,new Vector2(x,.035f),.045f); Box(r,new Vector2(x,-.025f),new Vector2(.038f,.07f)); }
        static void Person(Transform r,float x,float y) { Circle(r,new Vector2(x,y+.055f),.022f); Line(r,new Vector2(x,y+.03f),new Vector2(x,y-.025f)); Line(r,new Vector2(x-.035f,y+.005f),new Vector2(x+.035f,y+.005f)); Line(r,new Vector2(x-.025f,y-.06f),new Vector2(x,y-.025f),new Vector2(x+.025f,y-.06f)); }
        static void Arrow(Transform r,Vector2 from,Vector2 to) { Line(r,from,to); float sign=Mathf.Sign(to.y-from.y); Line(r,to+new Vector2(-.012f,-sign*.015f),to,to+new Vector2(.012f,-sign*.015f)); }
        static void Box(Transform r,Vector2 p,Vector2 s) { var a=p-s*.5f; var b=p+s*.5f; Line(r,a,new Vector2(b.x,a.y),b,new Vector2(a.x,b.y),a); }
        static void Circle(Transform r,Vector2 p,float radius)
        {
            var points=new Vector2[25]; for(int i=0;i<25;i++) {float a=i*Mathf.PI*2/24; points[i]=p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;} Line(r,points);
        }
        static void Line(Transform r,params Vector2[] points)
        {
            var go=new GameObject("Trazo"); go.transform.SetParent(r,false); var l=go.AddComponent<LineRenderer>(); l.useWorldSpace=false;
            l.sharedMaterial=RcpFinalSetup.White; l.startWidth=l.endWidth=.0035f; l.positionCount=points.Length;
            for(int i=0;i<points.Length;i++) l.SetPosition(i,new Vector3(points[i].x,points[i].y,0));
            l.numCapVertices=3;
        }
        static void Label(Transform r,string value,Vector2 at) => RcpFinalSetup.Text("Leyenda",r,new Vector3(at.x,at.y,-.001f),value,.19f,new Vector2(.45f,.035f));
    }
}
