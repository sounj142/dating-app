export interface Message {
  id: number;
  senderId: number;
  senderUserName: string;
  senderPhotoUrl: string;
  recipientId: number;
  recipientUserName: string;
  recipientPhotoUrl: string;
  content: string;
  dateRead?: Date;
  messageSent: Date;
}

export interface CreateMessageDto {
  recipientUserName: string;
  content: string;
}

export interface DateReadDto {
  messageId: number;
  dateRead?: Date;
}
