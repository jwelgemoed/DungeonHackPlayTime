using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;


namespace MapEditor.MapLogic
{
    public class VolumeBasedEditor
    {
        private float _midHeight;
        private float _midWidth;

        public VolumeBasedEditor(float MidHeight, float MidWidth)
        {
            _midHeight = MidHeight;
            _midWidth = MidWidth;
        }


        public void CreateVolumeFromRectangle(Rectangle  rectangle, float scaleFactor)
        {
            Model[] model = new Model[6];
            Vector3[] vectors = new Vector3[24];

            double left = Canvas.GetLeft(rectangle);
            double top = Canvas.GetTop(rectangle);
            double right = left + rectangle.Width;
            double bottom = top + rectangle.Height;


            //face1
            vectors[0].X = (float)(left * scaleFactor) - _midWidth;
            vectors[0].Y = 128.0f;
            vectors[0].Z = _midHeight - (float)(top * scaleFactor);

            vectors[1].X = (float)(right * scaleFactor) - _midWidth;
            vectors[1].Y = 128.0f;
            vectors[1].Z = _midHeight - (float)(top * scaleFactor);

            vectors[2].X = (float)(right * scaleFactor) - _midWidth;
            vectors[2].Y = 0.0f;
            vectors[2].Z = _midHeight - (float)(top * scaleFactor);

            vectors[3].X = (float)(left * scaleFactor) - _midWidth;
            vectors[3].Y = 0.0f;
            vectors[3].Z = _midHeight - (float)(top * scaleFactor);


            //face2
            vectors[4].X = (float)(left * scaleFactor) - _midWidth;
            vectors[4].Y = 128.0f;
            vectors[4].Z = _midHeight - (float)(bottom * scaleFactor);

            vectors[5].X = (float)(right * scaleFactor) - _midWidth;
            vectors[5].Y = 128.0f;
            vectors[5].Z = _midHeight - (float)(bottom * scaleFactor);

            vectors[6].X = (float)(right * scaleFactor) - _midWidth;
            vectors[6].Y = 0.0f;
            vectors[6].Z = _midHeight - (float)(bottom * scaleFactor);

            vectors[7].X = (float)(left * scaleFactor) - _midWidth;
            vectors[7].Y = 0.0f;
            vectors[7].Z = _midHeight - (float)(bottom * scaleFactor);


            //face3
            vectors[8].X = (float)(left * scaleFactor) - _midWidth;
            vectors[8].Y = 128.0f;
            vectors[8].Z = _midHeight - (float)(bottom * scaleFactor);

            vectors[9].X = (float)(left * scaleFactor) - _midWidth;
            vectors[9].Y = 128.0f;
            vectors[9].Z = _midHeight - (float)(top * scaleFactor);

            vectors[10].X = (float)(left * scaleFactor) - _midWidth;
            vectors[10].Y = 0.0f;
            vectors[10].Z = _midHeight - (float)(top * scaleFactor);

            vectors[11].X = (float)(left * scaleFactor) - _midWidth;
            vectors[11].Y = 0.0f;
            vectors[11].Z = _midHeight - (float)(bottom * scaleFactor);


            //face4
            vectors[12].X = (float)(right * scaleFactor) - _midWidth;
            vectors[12].Y = 128.0f;
            vectors[12].Z = _midHeight - (float)(bottom * scaleFactor);

            vectors[13].X = (float)(right * scaleFactor) - _midWidth;
            vectors[13].Y = 128.0f;
            vectors[13].Z = _midHeight - (float)(top * scaleFactor);

            vectors[14].X = (float)(right * scaleFactor) - _midWidth;
            vectors[14].Y = 0.0f;
            vectors[14].Z = _midHeight - (float)(top * scaleFactor);

            vectors[15].X = (float)(right * scaleFactor) - _midWidth;
            vectors[15].Y = 0.0f;
            vectors[15].Z = _midHeight - (float)(bottom * scaleFactor);



            //face5 
            vectors[16].X = (float)(left * scaleFactor) - _midWidth;
            vectors[16].Y = 128.0f;
            vectors[16].Z = _midHeight - (float)(top * scaleFactor);

            vectors[17].X = (float)(right * scaleFactor) - _midWidth;
            vectors[17].Y = 128.0f;
            vectors[17].Z = _midHeight - (float)(top * scaleFactor);

            vectors[18].X = (float)(right * scaleFactor) - _midWidth;
            vectors[18].Y = 128.0f;
            vectors[18].Z = _midHeight - (float)(top * scaleFactor);

            vectors[19].X = (float)(left * scaleFactor) - _midWidth;
            vectors[19].Y = 128.0f;
            vectors[19].Z = _midHeight - (float)(top * scaleFactor);


            //face6
            vectors[20].X = (float)(left * scaleFactor) - _midWidth;
            vectors[20].Y = 0.0f;
            vectors[20].Z = _midHeight - (float)(top * scaleFactor);

            vectors[21].X = (float)(right * scaleFactor) - _midWidth;
            vectors[21].Y = 0.0f;
            vectors[21].Z = _midHeight - (float)(top * scaleFactor);

            vectors[22].X = (float)(right * scaleFactor) - _midWidth;
            vectors[22].Y = 0.0f;
            vectors[22].Z = _midHeight - (float)(top * scaleFactor);

            vectors[23].X = (float)(left * scaleFactor) - _midWidth;
            vectors[23].Y = 0.0f;
            vectors[23].Z = _midHeight - (float)(top * scaleFactor);

            Vector3 normal = Vector3.Cross(vectors[0], vectors[1]);
            normal = Vector3.Normalize(normal);

            int counter = 0;
            foreach (var vector in vectors)
            {
                model[counter].x = vector.X;
                model[counter].y = vector.Y;
                model[counter].z = vector.Z;
                model[counter].nx = normal.X;
                model[counter].ny = normal.Y;
                model[counter].nz = normal.Z;

                switch (counter % 4)
                {
                    case 4:
                        model[counter].tx = 0.0f;
                        model[counter].ty = 0.0f;
                        break;
                    case 3:
                        model[counter].tx = 1.0f;
                        model[counter].ty = 0.0f;
                        break;
                    case 2:
                        model[counter].tx = 1.0f;
                        model[counter].ty = 1.0f;
                        break;
                    case 1:
                        model[counter].tx = 0.0f;
                        model[counter].ty = 0.0f;
                        break;
                    case 0:
                        model[counter].tx = 1.0f;
                        model[counter].ty = 1.0f;
                        break;
                }

                if (counter % 4 == 0)
                {
                    normal = Vector3.Cross(vectors[counter], vectors[counter + 1]);
                    normal = Vector3.Normalize(normal);
                }

                counter++;
            }

        }
    }
}
