import moment from 'moment';

export const terms = (key, terms) => {
    return { terms: { [key]: terms } };
}

const toISOString = (value) => {
    const dateValue = moment(value, moment.ISO_8601);
    if (value instanceof Date) {
        return value.getTime();
    } else if (dateValue.isValid()) {
        return dateValue.getTime();
    } else {
        return undefined;
    }
}

export const dateRange = (key, since, until) => {
    const sinceString = toISOString(since);
    const untilString = toISOString(until);

    return {
        range: {
            [key]: {
                gte: sinceString || 0,
                lte: untilString || 922337203685477590 
            }
        }
    };
}

export const sort = (keys) => {
    return keys.map(keyOrder => ({
        [keyOrder.key]: {
            order: keyOrder.descending || keyOrder.descending === undefined ? 'desc' : 'asc'
        }
    }));
}

export const bool = (must, must_not) => {
    return { bool: { must, must_not } };
}

export const search = (query, sort) => {
    return { query, sort };
}

