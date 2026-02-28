using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationDisplayUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI _messageText;

    [Header("Settings")]
    [SerializeField] private float _messageDuration = 2.5f;

    private Coroutine _displayCoroutine;

    private void Awake()
    {
        NotificationBus.OnStatusMessage += HandleNewMessage;

        _messageText = GetComponent<TextMeshProUGUI>();

        if (_messageText == null)
        {
            Debug.LogError("Notification display needs a TMP text component!", this);
        }
        else ResetText();
    }

    private void OnDestroy()
    {
        NotificationBus.OnStatusMessage -= HandleNewMessage;
    }

    private void HandleNewMessage(string message)
    {
        // If a message is currently showing, stop that timer
        if (_displayCoroutine != null) StopCoroutine(_displayCoroutine);

        // Start the new message routine
        _displayCoroutine = StartCoroutine(ShowMessageRoutine(message));
    }

    private IEnumerator ShowMessageRoutine(string text)
    {
        _messageText.text = text;

        yield return new WaitForSeconds(_messageDuration);

        ResetText();
    }

    private void ResetText()
    {
        _messageText.text = "";
        _displayCoroutine = null;
    }
}