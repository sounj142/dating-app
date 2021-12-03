export interface CacheItem<T> {
    timeExpired: Date;
    data: T
}