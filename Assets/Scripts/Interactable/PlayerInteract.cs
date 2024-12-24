using System.Collections.Generic;
using UnityEngine;

namespace Interactable
{
    public class PlayerInteract : MonoBehaviour
    {
        public IInteractable GetInteractableObject()
        {
            List<IInteractable> interactableList = new List<IInteractable>();
            float interactRange = 2f;
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
            foreach (Collider collider in colliderArray)
            {
                if (collider.TryGetComponent(out IInteractable interactable))
                {
                    interactableList.Add(interactable);
                }
            }
            IInteractable closestInteractable = null;
            foreach (IInteractable interactable in interactableList)
            {
                if (closestInteractable == null)
                {
                    closestInteractable = interactable;
                }
                else
                {
                    if (Vector3.Distance(transform.position, interactable.GetTransform().position) <
                        Vector3.Distance(transform.position, closestInteractable.GetTransform().position))
                    {
                        closestInteractable = interactable;
                    }
                }
            }
            return closestInteractable;
        }
    }
}