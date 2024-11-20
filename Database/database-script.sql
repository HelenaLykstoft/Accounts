CREATE TABLE "city"
(
    postal_code INT primary key NOT NULL,
    name        VARCHAR(255)    NOT NULL
);

CREATE TABLE "address"
(
    id            UUID primary key NOT NULL,
    street_number INT              NOT NULL,
    street_name   VARCHAR(255)     NOT NULL,
    city          INT              NOT NULL,
    CONSTRAINT fk_city FOREIGN KEY (city) REFERENCES "city" (postal_code) ON DELETE RESTRICT
);

CREATE TABLE "contact_info"
(
    id           UUID primary key NOT NULL,
    email        VARCHAR(255)     NOT NULL,
    phone_number VARCHAR(255)     NOT NULL,
    address      UUID,
    CONSTRAINT fk_address FOREIGN KEY (address) REFERENCES "address" (id) ON DELETE SET NULL
);

CREATE TABLE "user_type"
(
    id   INT primary key NOT NULL,
    type VARCHAR(255)    NOT NULL
);

CREATE TABLE "login_information"
(
    username VARCHAR(255) primary key NOT NULL,
    password VARCHAR(255) NOT NULL
);

CREATE TABLE "app_user"
(
    id           UUID primary key NOT NULL,
    first_name   VARCHAR(255)     NOT NULL,
    last_name    VARCHAR(255)     NOT NULL,
    username     VARCHAR(255)     UNIQUE NOT NULL,
    user_type    INT              NOT NULL,
    contact_info UUID             NOT NULL,
    CONSTRAINT fk_contact_info FOREIGN KEY (contact_info) REFERENCES "contact_info" (id) ON DELETE CASCADE,
    CONSTRAINT fk_user_type FOREIGN KEY (user_type) REFERENCES "user_type" (id) ON DELETE RESTRICT,
    CONSTRAINT fk_login_information FOREIGN KEY (username) REFERENCES "login_information" (username) ON DELETE CASCADE
);

CREATE TABLE "restaurant"
(
    id           UUID primary key NOT NULL,
    name         VARCHAR(255)     NOT NULL,
    username     VARCHAR(255)     NOT NULL,
    contact_info UUID             NOT NULL,
    CONSTRAINT fk_contact_info FOREIGN KEY (contact_info) REFERENCES "contact_info" (id) ON DELETE CASCADE,
    CONSTRAINT fk_login_information FOREIGN KEY (username) REFERENCES "login_information" (username) ON DELETE CASCADE
);

INSERT INTO "user_type" (id, type) VALUES
                                       (1, 'user'),
                                       (2, 'deliveryAgent'),
                                       (3, 'admin');
