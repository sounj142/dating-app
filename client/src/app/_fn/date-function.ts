export function formatDateTimeWithLocalTimezone(date: Date): string {
  return _formatDate(date, _formatTime);
}

export function formatDateWithLocalTimezone(date: Date): string {
  return _formatDate(date, () => '00:00:00');
}

function _formatDate(date: Date, formatTimeFunc): string {
  const year = date.getFullYear();
  const month = ('0' + (date.getMonth() + 1)).slice(-2);
  const day = ('0' + date.getDate()).slice(-2);

  let timeZone = '';
  const dateString = date.toString();
  const gmtIndex = dateString.indexOf('GMT');
  const nextSpaceIndex = dateString.indexOf(' ', gmtIndex);
  if (gmtIndex >= 0 && nextSpaceIndex >= 0) {
    timeZone = dateString.substring(gmtIndex + 3, nextSpaceIndex);
    timeZone = `${timeZone.slice(0, timeZone.length - 2)}:${timeZone.slice(
      timeZone.length - 2
    )}`;
  }
  const timeFormatString = formatTimeFunc(date);

  return `${year}-${month}-${day}T${timeFormatString}${timeZone}`;
}

function _formatTime(date: Date) {
  const hour = ('0' + date.getHours()).slice(-2);
  const minute = ('0' + date.getMinutes()).slice(-2);
  const second = ('0' + date.getSeconds()).slice(-2);

  return `${hour}:${minute}:${second}`;
}
