using Interactable;
using UnityEngine;
using TMPro;

namespace UI
{
    public class PlayerInteractUI : MonoBehaviour
    {
        [SerializeField] private GameObject containerGameObject;
        [SerializeField] private PlayerInteract playerInteract;
        [SerializeField] private TextMeshProUGUI interactText;

        private void Update()
        {
            if (playerInteract.GetInteractableObject() != null)
            {
                Show(playerInteract.GetInteractableObject());
            }
            else
            {
                Hide();
            }
        }

        private void Show(IInteractable interactable)
        {
            containerGameObject.SetActive(true);
            interactText.text = interactable.GetInteractText();
        }

        private void Hide()
        {
            containerGameObject.SetActive(false);
        }
    }
}