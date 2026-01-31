using UnityEngine;
using UnitSystem;
using UnityEngine.UI;
using System.Text;

namespace UnitSystem.Demo
{

    public class ConsoleUI : MonoBehaviour
    {

        [SerializeField]
        private APlayer player;

        [SerializeField]
        private Text text;

        [SerializeField]
        private ScrollRect scrollRect;

        private StringBuilder stringBuilder = new StringBuilder();

        private void OnEnable()
        {
            player.OnProducableAdded += PrintProducableAdded;
            player.OnProducableRemoved += PrintProducableRemoved;
        }

        private void OnDisable()
        {
            player.OnProducableAdded -= PrintProducableAdded;
            player.OnProducableRemoved -= PrintProducableRemoved;
        }

        private void Start()
        {
            AppendText("Game started...");
        }

        private void Update()
        {
            scrollRect.verticalNormalizedPosition = 0;
        }

        private void PrintProducableAdded(ProducableSO producable, float quantity)
        {
            switch (producable)
            {
                case ResourceSO _: break;

                // In this demo, structures are the only ones that produce and
                // we do not differentiate from other units by type. Generally
                // this would be needed.
                case ProductionUnitSO productionUnit:
                    AppendText("Built " + productionUnit.Name);
                    break;

                case AUnitSO unit:
                    AppendText("Trained " + quantity + " " + unit.Name);
                    break;

                case ResearchSO research:
                    AppendText("Researched " + research.Name);
                    break;
            }
        }

        private void PrintProducableRemoved(ProducableSO producable)
        {
            AppendText("Lost " + producable.Name + "...");
        }

        private void AppendText(string text)
        {
            int seconds = (int)(Time.time % 60f);
            int minutes = (int)(Time.time / 60f);

            stringBuilder.Append("[");
            stringBuilder.Append(minutes);
            stringBuilder.Append(":");
            stringBuilder.Append(seconds);
            stringBuilder.Append("]: ");
            stringBuilder.Append(text);
            stringBuilder.Append("\n");

            this.text.text = stringBuilder.ToString();
        }

    }

}